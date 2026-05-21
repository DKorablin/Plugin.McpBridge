import "./index.scss";
import { HttpAgent } from "@ag-ui/client";
import type { AssistantMessage, Message, RunAgentInput, ToolMessage } from "@ag-ui/core";
import { EventType } from "@ag-ui/core";

const messagesEl = document.getElementById("messages")!;
const inputEl = document.getElementById("input") as HTMLInputElement;
const sendBtn = document.getElementById("send") as HTMLButtonElement;
const attachBtn = document.getElementById("attach-btn") as HTMLButtonElement;
const fileInput = document.getElementById("file-input") as HTMLInputElement;
const previewContainer = document.getElementById("attachment-preview-container")!;

// Thread ID is stable across runs within a conversation and persisted in localStorage.
let currentThreadId = localStorage.getItem("ag-ui-thread-id") ?? crypto.randomUUID();
localStorage.setItem("ag-ui-thread-id", currentThreadId);
const history: Message[] = [];

let selectedFiles: File[] = [];// Track files staged for sending
// Handle file staging mechanics
attachBtn.addEventListener("click", () => fileInput.click());
fileInput.addEventListener("change", () => {
	if (!fileInput.files) return;
	for (const file of Array.from(fileInput.files)) {
		selectedFiles.push(file);
	}
	renderPreviews();
	fileInput.value = ""; // Clear input to allow re-uploading the same file
});

function renderPreviews(): void {
	previewContainer.innerHTML = "";
	selectedFiles.forEach((file, index) => {
		const badge = document.createElement("div");
		badge.className = "file-preview-badge";
		badge.innerHTML = `<span>&#128196; ${file.name}</span><span class="remove-file" data-index="${index}">&times;</span>`;
		badge.querySelector(".remove-file")!.addEventListener("click", (e) => {
			const idx = parseInt((e.target as HTMLElement).dataset.index!);
			selectedFiles.splice(idx, 1);
			renderPreviews();
		});
		previewContainer.appendChild(badge);
	});
}

// Utility helper to encode local browser files to base64 Data URLs
function fileToDataUrl(file: File): Promise<string> {
	return new Promise((resolve, reject) => {
		const reader = new FileReader();
		reader.onload = () => resolve(reader.result as string);
		reader.onerror = (err) => reject(err);
		reader.readAsDataURL(file);
	});
}

// ── Helpers ───────────────────────────────────────────────────────────────────

function appendMessage(role: "user" | "assistant", text: string): HTMLElement {
	const el = document.createElement("div");
	el.className = `msg ${role}`;
	el.textContent = text;
	messagesEl.appendChild(el);
	messagesEl.scrollTop = messagesEl.scrollHeight;
	return el;
}

// ── History persistence ───────────────────────────────────────────────────────

function renderHistory(): void {
	for (const msg of history) {
		if (msg.role === "user" && typeof msg.content === "string")
			appendMessage("user", msg.content);
		else if (msg.role === "assistant" && typeof msg.content === "string" && msg.content.length > 0)
			appendMessage("assistant", msg.content);
	}
}

async function loadHistory(): Promise<void> {
	const resp = await fetch(`/history/${encodeURIComponent(currentThreadId)}`);
	if (!resp.ok) return;

	type SessionContent = { $type: string; text?: string };
	type SessionMessage = { role: string; contents: SessionContent[]; messageId?: string };

	const sessionMessages = (await resp.json()) as SessionMessage[];
	const seen = new Set<string>();

	for (const msg of sessionMessages) {
		const id = msg.messageId ?? crypto.randomUUID();
		if (seen.has(id)) continue;
		seen.add(id);

		const text = msg.contents.find(c => c.$type === "text")?.text;
		if (!text) continue;

		if (msg.role === "user")
			history.push({ id, role: "user", content: text });
		else if (msg.role === "assistant")
			history.push({ id, role: "assistant", content: text });
	}

	if (history.length > 0)
		renderHistory();
}

// ── Approval card ─────────────────────────────────────────────────────────────

interface PendingApproval {
	toolCallId: string;
	approvalId: string;
	functionName: string;
	resolve: (approved: boolean) => void;
}

const pendingApprovals = new Map<string, PendingApproval>();

function showApprovalCard(approval: PendingApproval): void {
	const card = document.createElement("div");
	card.className = "msg approval-request";
	card.dataset.toolCallId = approval.toolCallId;
	card.innerHTML = `
		<div class="approval-header"><span>&#9888;</span> Approval Required</div>
		<div class="approval-tool">Tool: <code>${approval.functionName}</code></div>
		<div class="approval-actions">
			<button class="approve-btn">Approve</button>
			<button class="deny-btn">Deny</button>
		</div>`;
	card.querySelector(".approve-btn")!.addEventListener("click", () => {
		card.remove();
		pendingApprovals.delete(approval.toolCallId);
		approval.resolve(true);
	});
	card.querySelector(".deny-btn")!.addEventListener("click", () => {
		card.remove();
		pendingApprovals.delete(approval.toolCallId);
		approval.resolve(false);
	});
	messagesEl.appendChild(card);
	messagesEl.scrollTop = messagesEl.scrollHeight;
}

// ── Run loop ──────────────────────────────────────────────────────────────────

async function runAgent(input: RunAgentInput, assistantEl: HTMLElement): Promise<void> {
	const agent = new HttpAgent({ url: "/agui" });
	let assistantText = assistantEl.textContent === "…" ? "" : (assistantEl.textContent ?? "");

	// Accumulate streamed tool call args per callId
	const toolCallArgs = new Map<string, string>();
	const toolCallNames = new Map<string, string>();

	// Collect "request_approval" tool calls emitted during this run (for history)
	const emittedApprovalCalls: { id: string; name: string; args: string }[] = [];

	await new Promise<void>((resolveRun, rejectRun) => {
		agent.run(input).subscribe({
			next: (event) => {
				switch (event.type) {
					case EventType.TEXT_MESSAGE_CONTENT:
						assistantText += (event as { delta?: string }).delta ?? "";
						assistantEl.textContent = assistantText;
						messagesEl.scrollTop = messagesEl.scrollHeight;
						break;

					case EventType.TOOL_CALL_START: {
						const e = event as { toolCallId: string; toolCallName: string };
						toolCallNames.set(e.toolCallId, e.toolCallName);
						toolCallArgs.set(e.toolCallId, "");
						break;
					}

					case EventType.TOOL_CALL_ARGS: {
						const e = event as { toolCallId: string; delta: string };
						toolCallArgs.set(e.toolCallId, (toolCallArgs.get(e.toolCallId) ?? "") + e.delta);
						break;
					}

					case EventType.TOOL_CALL_END: {
						const e = event as { toolCallId: string };
						const callId = e.toolCallId;
						const name = toolCallNames.get(callId) ?? "";
						const argsRaw = toolCallArgs.get(callId) ?? "{}";

						if (name !== "request_approval") break;

						emittedApprovalCalls.push({ id: callId, name, args: argsRaw });

						let approvalId = callId;
						let functionName = "unknown";
						try {
							const outer = JSON.parse(argsRaw) as { request?: string };
							if (outer.request) {
								const inner = JSON.parse(outer.request) as {
									approval_id?: string;
									function_name?: string;
								};
								approvalId = inner.approval_id ?? callId;
								functionName = inner.function_name ?? "unknown";
							}
						} catch { /* keep defaults */ }

						// Show card; when user clicks, continue the agent run
						const approval: PendingApproval = {
							toolCallId: callId,
							approvalId,
							functionName,
							resolve: async (approved: boolean) => {
								// Build tool result message
								const assistantMsg: AssistantMessage = {
									id: crypto.randomUUID(),
									role: "assistant",
									toolCalls: emittedApprovalCalls.map(c => ({
										id: c.id,
										type: "function" as const,
										function: { name: c.name, arguments: c.args },
									})),
								};
								const toolResult: ToolMessage = {
									id: crypto.randomUUID(),
									role: "tool",
									toolCallId: callId,
									content: JSON.stringify({ approval_id: approvalId, approved }),
								};
								const nextInput: RunAgentInput = {
									...input,
									runId: crypto.randomUUID(),
									messages: [...input.messages, assistantMsg, toolResult],
								};
								await runAgent(nextInput, assistantEl);
							},
						};
						pendingApprovals.set(callId, approval);
						showApprovalCard(approval);
						break;
					}
				}
			},
			error: (err: unknown) => {
				assistantEl.textContent = `Error: ${err}`;
				sendBtn.disabled = false;
				rejectRun(err);
			},
			complete: () => resolveRun(),
		});
	});
}

// ── Send ──────────────────────────────────────────────────────────────────────

async function send(): Promise<void> {
	const text = inputEl.value.trim();
	if (!text && selectedFiles.length === 0) return;

	inputEl.value = "";
	sendBtn.disabled = true;
	attachBtn.disabled = true;

	const contentArray: any[] = [];

	if (text)
		contentArray.push({ $type: "text", text: text });

	const displayNames: string[] = [];

	// Warning: Sending files using AG-UI is not working in all contexts. The server is rejecting all attempts to send files.
	for (const file of selectedFiles) {
		displayNames.push(file.name);
		const dataUrl = await fileToDataUrl(file);
		const mimeType = file.type || "application/octet-stream";

		if (mimeType.startsWith("image/")) {
			contentArray.push({
				$type: "image",
				image: { url: dataUrl }
			});
		} else {
			contentArray.push({
				$type: "file",
				file: { url: dataUrl, contentType: mimeType }
			});
		}
	}

	const visualText = text + (displayNames.length > 0 ? `\n\n[Attached: ${displayNames.join(", ")}]` : "");
	const uiRenderText = selectedFiles.length > 0 ? `${text} [File Attached]` : text;

	selectedFiles = [];
	previewContainer.innerHTML = "";

const userMessage: Message = {
		id: crypto.randomUUID(),
		role: "user",
		content: visualText,
		contents: contentArray as any
	};

	history.push(userMessage);
	appendMessage("user", uiRenderText);

	const assistantEl = appendMessage("assistant", "…");

	const input: RunAgentInput = {
		threadId: currentThreadId,
		runId: crypto.randomUUID(),
		messages: [userMessage],
		tools: [],
		context: [],
	};

	try {
		await runAgent(input, assistantEl);

		if (assistantEl.textContent && assistantEl.textContent !== "…")
			history.push({ id: crypto.randomUUID(), role: "assistant", content: assistantEl.textContent });
	} finally {
		sendBtn.disabled = false;
		attachBtn.disabled = false;
		inputEl.focus();
	}
}

sendBtn.addEventListener("click", send);
inputEl.addEventListener("keydown", (e) => { if (e.key === "Enter" && !e.shiftKey) void send(); });
void loadHistory();
inputEl.focus();
import "./index.scss";
import { HttpAgent } from "@ag-ui/client";
import type { AssistantMessage, Message, RunAgentInput, ToolMessage } from "@ag-ui/core";
import { EventType } from "@ag-ui/core";

const messagesEl = document.getElementById("messages")!;
const inputEl = document.getElementById("input") as HTMLInputElement;
const sendBtn = document.getElementById("send") as HTMLButtonElement;

// Thread ID is stable across runs within a conversation.
let currentThreadId = crypto.randomUUID();
const history: Message[] = [];

// ── Helpers ───────────────────────────────────────────────────────────────────

function appendMessage(role: "user" | "assistant", text: string): HTMLElement {
	const el = document.createElement("div");
	el.className = `msg ${role}`;
	el.textContent = text;
	messagesEl.appendChild(el);
	messagesEl.scrollTop = messagesEl.scrollHeight;
	return el;
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
	if (!text) return;

	inputEl.value = "";
	sendBtn.disabled = true;

	const userMessage: Message = { id: crypto.randomUUID(), role: "user", content: text };
	history.push(userMessage);
	appendMessage("user", text);

	const assistantEl = appendMessage("assistant", "…");

	const input: RunAgentInput = {
		threadId: currentThreadId,
		runId: crypto.randomUUID(),
		messages: [...history],
		tools: [],
		context: [],
	};

	try {
		await runAgent(input, assistantEl);
	} finally {
		if (assistantEl.textContent && assistantEl.textContent !== "…")
			history.push({ id: crypto.randomUUID(), role: "assistant", content: assistantEl.textContent });
		sendBtn.disabled = false;
		inputEl.focus();
	}
}

sendBtn.addEventListener("click", send);
inputEl.addEventListener("keydown", (e) => { if (e.key === "Enter" && !e.shiftKey) void send(); });
inputEl.focus();
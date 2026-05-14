import { HttpAgent } from "@ag-ui/client";
import type { Message } from "@ag-ui/core";
import { EventType } from "@ag-ui/core";

const messagesEl = document.getElementById("messages")!;
const inputEl = document.getElementById("input") as HTMLInputElement;
const sendBtn = document.getElementById("send") as HTMLButtonElement;

const history: Message[] = [];

function appendMessage(role: "user" | "assistant", text: string): HTMLElement {
	const el = document.createElement("div");
	el.className = `msg ${role}`;
	el.textContent = text;
	messagesEl.appendChild(el);
	messagesEl.scrollTop = messagesEl.scrollHeight;
	return el;
}

async function send() {
	const text = inputEl.value.trim();
	if (!text) return;

	inputEl.value = "";
	sendBtn.disabled = true;

	const userMessage: Message = {
		id: crypto.randomUUID(),
		role: "user",
		content: text,
	};
	history.push(userMessage);
	appendMessage("user", text);

	const assistantEl = appendMessage("assistant", "…");
	let assistantText = "";

	const agent = new HttpAgent({ url: "/agui" });

	agent
		.run({
			threadId: crypto.randomUUID(),
			runId: crypto.randomUUID(),
			messages: [...history],
			tools: [],
			context: [],
		})
		.subscribe({
			next: (event) => {
				if (event.type === EventType.TEXT_MESSAGE_CONTENT) {
					assistantText += event.delta ?? "";
					assistantEl.textContent = assistantText;
					messagesEl.scrollTop = messagesEl.scrollHeight;
				}
			},
			error: (err) => {
				assistantEl.textContent = `Error: ${err}`;
				sendBtn.disabled = false;
			},
			complete: () => {
				if (assistantText) {
					history.push({
						id: crypto.randomUUID(),
						role: "assistant",
						content: assistantText,
					});
				}
				sendBtn.disabled = false;
				inputEl.focus();
			},
		});
}

sendBtn.addEventListener("click", send);
inputEl.addEventListener("keydown", (e) => {
	if (e.key === "Enter" && !e.shiftKey) send();
});
inputEl.focus();
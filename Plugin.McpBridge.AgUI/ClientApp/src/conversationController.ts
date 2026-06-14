export interface ConversationControllerOptions {
	conversationSelect: HTMLSelectElement;
	newConversationBtn: HTMLButtonElement;
	deleteConversationBtn: HTMLButtonElement;
	onConversationSwitched: () => Promise<void>;
	clearConversationState: () => void;
}

export class ConversationController {
	private readonly conversationSelect: HTMLSelectElement;
	private readonly newConversationBtn: HTMLButtonElement;
	private readonly deleteConversationBtn: HTMLButtonElement;
	private readonly onConversationSwitched: () => Promise<void>;
	private readonly clearConversationState: () => void;

	private currentThreadId: string;
	private conversationListLoaded = false;

	constructor(options: ConversationControllerOptions) {
		this.conversationSelect = options.conversationSelect;
		this.newConversationBtn = options.newConversationBtn;
		this.deleteConversationBtn = options.deleteConversationBtn;
		this.onConversationSwitched = options.onConversationSwitched;
		this.clearConversationState = options.clearConversationState;

		this.currentThreadId = localStorage.getItem("ag-ui-thread-id") ?? crypto.randomUUID();
		localStorage.setItem("ag-ui-thread-id", this.currentThreadId);
		this.bindEvents();
	}

	public initialize(): void {
		this.updateSelectValue(this.currentThreadId);
	}

	public getCurrentThreadId(): string {
		return this.currentThreadId;
	}

	private setCurrentConversation(id: string): void {
		this.currentThreadId = id;
		localStorage.setItem("ag-ui-thread-id", id);
		this.updateSelectValue(id);
	}

	private updateSelectValue(id: string): void {
		const escapedId = CSS.escape(id);
		const matchingOptions = this.conversationSelect.querySelectorAll<HTMLOptionElement>(`option[value="${escapedId}"]`);
		if (matchingOptions.length > 1) {
			for (let i = 1; i < matchingOptions.length; i++)
				matchingOptions[i].remove();
		}

		let opt = this.conversationSelect.querySelector<HTMLOptionElement>(`option[value="${escapedId}"]`);
		if (!opt) {
			opt = document.createElement("option");
			opt.value = id;
			opt.textContent = id;
			this.conversationSelect.appendChild(opt);
		}
		this.conversationSelect.value = id;
	}

	private async populateConversationList(): Promise<void> {
		if (this.conversationListLoaded)
			return;

		let ids: string[] = [];
		const resp = await fetch("/history");
		if (resp.ok)
			ids = (await resp.json()) as string[];

		const all = new Set([...ids, this.currentThreadId]);
		this.conversationSelect.innerHTML = "";
		for (const id of all) {
			const opt = document.createElement("option");
			opt.value = id;
			opt.textContent = id;
			this.conversationSelect.appendChild(opt);
		}

		this.conversationSelect.value = this.currentThreadId;
		this.conversationListLoaded = true;
	}

	private bindEvents(): void {
		this.conversationSelect.addEventListener("mousedown", (e) => {
			if (this.conversationListLoaded)
				return;

			e.preventDefault();
			void (async () => {
				await this.populateConversationList();
				this.conversationSelect.showPicker();
			})();
		});

		this.conversationSelect.addEventListener("change", async () => {
			const selectedId = this.conversationSelect.value;
			if (!selectedId || selectedId === this.currentThreadId)
				return;

			this.setCurrentConversation(selectedId);
			this.clearConversationState();
			await this.onConversationSwitched();
		});

		this.newConversationBtn.addEventListener("click", () => {
			const newId = crypto.randomUUID();
			this.setCurrentConversation(newId);
			this.clearConversationState();
		});

		this.deleteConversationBtn.addEventListener("click", async () => {
			const idToDelete = this.currentThreadId;
			if (!confirm(`Delete conversation\n${idToDelete}?\n\nThis cannot be undone.`))
				return;

			const optionValues = Array.from(this.conversationSelect.options).map(o => o.value);
			const currentIndex = optionValues.findIndex(v => v === idToDelete);
			const previousCachedId = currentIndex > 0 ? optionValues[currentIndex - 1] : null;

			try {
				await fetch(`/history/${encodeURIComponent(idToDelete)}`, { method: "DELETE" });
			} catch {
				// Ignore network errors - file already gone is fine.
			}

			this.conversationSelect.querySelector<HTMLOptionElement>(`option[value="${CSS.escape(idToDelete)}"]`)?.remove();

			let serverIds: string[] = [];
			const resp = await fetch("/history");
			if (resp.ok)
				serverIds = (await resp.json()) as string[];

			const cachedIds = Array.from(this.conversationSelect.options).map(o => o.value);
			const availableIds = Array.from(new Set([...serverIds, ...cachedIds])).filter(id => id && id !== idToDelete);
			const nextId = previousCachedId && availableIds.includes(previousCachedId)
				? previousCachedId
				: (availableIds.length > 0 ? availableIds[0] : null);

			if (nextId) {
				this.setCurrentConversation(nextId);
				this.clearConversationState();
				await this.onConversationSwitched();
				return;
			}

			const newId = crypto.randomUUID();
			this.setCurrentConversation(newId);
			this.clearConversationState();
		});
	}
}
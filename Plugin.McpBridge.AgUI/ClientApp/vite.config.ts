import { defineConfig } from "vite";

export default defineConfig({
	build: {
		outDir: "../wwwroot",
		emptyOutDir: true,
		lib: {
			entry: "src/main.ts",
			name: "aguiClient",
			fileName: "agui-client",
			formats: ["es"],
		},
		rollupOptions: {
			output: {
				entryFileNames: "agui-client.js",
				assetFileNames: "index[extname]",
			},
		},
	},
});
import path from "path";
import { defineConfig, type Plugin } from "vite";
import react from "@vitejs/plugin-react-swc";
import tailwindcss from "@tailwindcss/vite";

function transferMeta(htmlServer: string, htmlLocal: string): string {
  const titleMatch = htmlServer.match(/<title[^>]*>(.*?)<\/title>/i);
  const serverTitle = titleMatch ? titleMatch[1] : null;

  let result = htmlLocal;

  if (serverTitle) {
    result = result.replace(/<title[^>]*>.*?<\/title>/i, `<title>${serverTitle}</title>`);
  }

  // Transfer ivy-* meta tags
  const ivyMetaMatches = htmlServer.match(/<meta[^>]*name\s*=\s*["']ivy-[^"']*["'][^>]*>/gi);

  // Transfer ivy-custom-theme style tag
  const themeStyleMatch = htmlServer.match(/<style id="ivy-custom-theme">[\s\S]*?<\/style>/i);

  if (ivyMetaMatches || themeStyleMatch) {
    const headEndIndex = result.indexOf("</head>");
    if (headEndIndex !== -1) {
      let toInsert = "";

      if (ivyMetaMatches) {
        toInsert += ivyMetaMatches.map((meta) => ` ${meta}`).join("\n");
      }

      if (themeStyleMatch) {
        if (toInsert) toInsert += "\n";
        toInsert += ` ${themeStyleMatch[0]}`;
      }

      result = result.slice(0, headEndIndex) + toInsert + "\n " + result.slice(headEndIndex);
    }
  }

  return result;
}

const injectMeta = (mode: string): Plugin => {
  return {
    name: "inject-ivy-meta",
    async transformIndexHtml(localHtml: string) {
      if (mode === "development") {
        const host = process.env.IVY_HOST || "http://localhost:5010";
        const serverHtml = await fetch(`${host}`).then((res) => res.text());
        const transformedHtml = transferMeta(serverHtml, localHtml);
        const ivyHostTag = `<meta name="ivy-host" content="${host}" />`;
        return transformedHtml.replace("</head>", ` ${ivyHostTag}\n</head>`);
      }
      return localHtml;
    },
  };
};

export default defineConfig(({ mode }) => ({
  lint: {
    "plugins": [
      "oxc",
      "typescript",
      "unicorn",
      "react"
    ],
    "categories": {
      "correctness": "warn"
    },
    "env": {
      "builtin": true
    },
    "ignorePatterns": [
      "dist"
    ],
    "overrides": [
      {
        "files": [
          "**/*.{ts,tsx}"
        ],
        "rules": {
          "constructor-super": "off",
          "for-direction": "error",
          "getter-return": "off",
          "no-async-promise-executor": "error",
          "no-case-declarations": "error",
          "no-class-assign": "off",
          "no-compare-neg-zero": "error",
          "no-cond-assign": "error",
          "no-const-assign": "off",
          "no-constant-binary-expression": "error",
          "no-constant-condition": "error",
          "no-control-regex": "error",
          "no-debugger": "error",
          "no-delete-var": "error",
          "no-dupe-class-members": "off",
          "no-dupe-else-if": "error",
          "no-dupe-keys": "off",
          "no-duplicate-case": "error",
          "no-empty": "error",
          "no-empty-character-class": "error",
          "no-empty-pattern": "error",
          "no-empty-static-block": "error",
          "no-ex-assign": "error",
          "no-extra-boolean-cast": "error",
          "no-fallthrough": "error",
          "no-func-assign": "off",
          "no-global-assign": "error",
          "no-import-assign": "off",
          "no-invalid-regexp": "error",
          "no-irregular-whitespace": "error",
          "no-loss-of-precision": "error",
          "no-misleading-character-class": "error",
          "no-new-native-nonconstructor": "off",
          "no-nonoctal-decimal-escape": "error",
          "no-obj-calls": "off",
          "no-prototype-builtins": "error",
          "no-redeclare": "off",
          "no-regex-spaces": "error",
          "no-self-assign": "error",
          "no-setter-return": "off",
          "no-shadow-restricted-names": "error",
          "no-sparse-arrays": "error",
          "no-this-before-super": "off",
          "no-undef": "off",
          "no-unexpected-multiline": "error",
          "no-unreachable": "off",
          "no-unsafe-finally": "error",
          "no-unsafe-negation": "off",
          "no-unsafe-optional-chaining": "error",
          "no-unused-labels": "error",
          "no-unused-private-class-members": "error",
          "no-unused-vars": "error",
          "no-useless-backreference": "error",
          "no-useless-catch": "error",
          "no-useless-escape": "error",
          "no-with": "off",
          "require-yield": "error",
          "use-isnan": "error",
          "valid-typeof": "error",
          "no-var": "error",
          "prefer-const": "error",
          "prefer-rest-params": "error",
          "prefer-spread": "error",
          "@typescript-eslint/ban-ts-comment": "error",
          "no-array-constructor": "error",
          "@typescript-eslint/no-duplicate-enum-values": "error",
          "@typescript-eslint/no-empty-object-type": "error",
          "@typescript-eslint/no-explicit-any": "error",
          "@typescript-eslint/no-extra-non-null-assertion": "error",
          "@typescript-eslint/no-misused-new": "error",
          "@typescript-eslint/no-namespace": "error",
          "@typescript-eslint/no-non-null-asserted-optional-chain": "error",
          "@typescript-eslint/no-require-imports": "error",
          "@typescript-eslint/no-this-alias": "error",
          "@typescript-eslint/no-unnecessary-type-constraint": "error",
          "@typescript-eslint/no-unsafe-declaration-merging": "error",
          "@typescript-eslint/no-unsafe-function-type": "error",
          "no-unused-expressions": "error",
          "@typescript-eslint/no-wrapper-object-types": "error",
          "@typescript-eslint/prefer-as-const": "error",
          "@typescript-eslint/prefer-namespace-keyword": "error",
          "@typescript-eslint/triple-slash-reference": "error",
          "react-hooks/rules-of-hooks": "error",
          "react-hooks/exhaustive-deps": "warn",
          "prettier/prettier": "error",
          "no-restricted-imports": [
            "error",
            {
              "patterns": [
                {
                  "group": [
                    "@/widgets*",
                    "@/components*",
                    "@/lib*",
                    "@/hooks*",
                    "@/services*"
                  ],
                  "importNames": [
                    "*"
                  ],
                  "message": "Wildcard imports from internal modules are not allowed. Use named imports instead."
                }
              ]
            }
          ],
          "@typescript-eslint/no-import-type-side-effects": "error",
          "react/only-export-components": [
            "warn",
            {
              "allowConstantExport": true
            }
          ]
        },
        "jsPlugins": [
          "eslint-plugin-prettier"
        ],
        "env": {
          "es2020": true,
          "browser": true
        },
        "globals": {
          "AudioWorkletGlobalScope": "readonly",
          "AudioWorkletProcessor": "readonly",
          "currentFrame": "readonly",
          "currentTime": "readonly",
          "registerProcessor": "readonly",
          "sampleRate": "readonly",
          "WorkletGlobalScope": "readonly"
        }
      }
    ],
    "options": {}
  },
  fmt: {
    semi: true,
    trailingComma: "es5",
    singleQuote: true,
    printWidth: 80,
    tabWidth: 2,
    useTabs: false,
    bracketSpacing: true,
    bracketSameLine: false,
    arrowParens: "avoid",
    endOfLine: "lf",
    sortPackageJson: false,
    ignorePatterns: [
      "node_modules/",
      "dist/",
      "build/",
      "*.min.js",
      "*.min.css",
      "*.log",
      "npm-debug.log*",
      "yarn-debug.log*",
      "yarn-error.log*",
      "pids",
      "*.pid",
      "*.seed",
      "*.pid.lock",
      "coverage/",
      ".nyc_output",
      "jspm_packages/",
      ".npm",
      ".node_repl_history",
      "*.tgz",
      ".yarn-integrity",
      ".env",
      "playwright-report/",
      "test-results/",
      "package-lock.json",
      "*.d.ts",
    ],
  },
  plugins: [react(), tailwindcss(), injectMeta(mode)] as Plugin[],
  server: {
    proxy: {
      "^/(.*\\.md|llms\\.txt)$": {
        target: process.env.IVY_HOST || "http://localhost:5010",
        changeOrigin: true,
      },
    },
  },
  esbuild: {
    drop: process.env.NODE_ENV === "production" ? ["console", "debugger"] : [],
    legalComments: "none",
  },
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  build: {
    target: "es2020",
    outDir: "dist",
    assetsDir: "assets",
    cssCodeSplit: true,
    sourcemap: false,
    rollupOptions: {
      input: {
        main: path.resolve(__dirname, "index.html"),
      },
      output: {
        entryFileNames: "assets/[name].[hash].js",
        chunkFileNames: "assets/[name].[hash].js",
        assetFileNames: "assets/[name].[hash].[ext]",
      },
    },
  },
  test: {
    include: ["**/*.test.ts"],
    exclude: ["**/e2e/**", "**/node_modules/**", "**/dist/**"],
    environment: "happy-dom",
  },
}));

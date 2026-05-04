# Copilot Instructions

These instructions apply to **every** code suggestion, refactor, generation, or
review. Treat each rule as a hard requirement. If two rules appear to conflict,
follow them in the order listed (Section 1 has the highest priority).

---

## 1. Response Format

### 1.1 Line numbers must be absolute

When suggesting code changes or refactors, every line number you provide MUST be
the **absolute line number within the entire file**, counted from line `1` at
the very top of the file.

The count includes:
- `using` statements and directives
- Blank lines
- Comments
- `<style>` and `<script>` blocks
- Razor directives (`@page`, `@model`, `@using`, etc.)

You MUST NOT use line numbers that are relative to:
- A code block, snippet, function, method, class, or region
- The visible / scrolled / selected portion of the file
- The chat message itself

#### Required citation format

| Scenario                  | Required wording                                                  |
|---------------------------|-------------------------------------------------------------------|
| Single-line change        | `Apply this change at **line 45** of` `` `Path\To\File.ext` ``    |
| Contiguous range          | `Replace **lines 45–62** of` `` `Path\To\File.ext` ``             |
| Multiple non-contiguous   | List each edit on its own line with its own absolute line number. |

#### Forbidden phrasings

- "line 45 of the method"
- "line 10 of the snippet"
- "around line 45"
- "near the top of the function"

#### When you are uncertain

If the full file was not provided and you cannot determine the absolute line
number with certainty, you MUST say so explicitly and ask the user for the full
file. Do not guess. Do not silently fall back to relative numbering.

### 1.2 Always show After / Diff blocks

When suggesting any code modification, always provide **two distinct, fenced
code blocks** in this order:

1. A block labeled `After` containing the modified code, fenced with the
   source language (e.g., ```` ```razor ````, ```` ```csharp ````).
2. A block labeled `Diff` containing a plain unified diff showing the changes
   from the original code to the `After` code. Fence it as ```` ```diff ````
   so the syntax is recognized, but **do not assume any color rendering** —
   the Visual Studio Copilot Chat panel does not apply red/green backgrounds
   to `diff` fences. The block exists for readability and quick scanning, not
   for visual highlighting.

The label (After/Diff) MUST NOT appear inside the fenced code block, because Visual Studio's Copy-code button copies the
entire fence contents and any label text inside the fence becomes part of the
copied code.

Never present changes in isolation or as prose alone.

#### 1.2.1 Required syntax inside the `Diff` block

Every line inside the ```` ```diff ```` block MUST begin with one of the
following three prefixes — no other prefixes are allowed, and no line may have
an empty / blank prefix:

| Prefix       | Meaning             |
|--------------|---------------------|
| `-` + space  | Removed line        |
| `+` + space  | Added line          |
| ` ` (space)  | Unchanged context   |

Additional rules:
- Include **2–3 lines of unchanged context** (` ` prefix) above and below each
  modified region so the change can be located by eye.
- For multiple non-contiguous edits in the same file, separate them with a
  unified-diff hunk header on its own line, e.g.:
  `@@ line 142 @@` or `@@ -142,5 +142,6 @@`
- A truly blank line in the source code MUST still carry a leading prefix
  (e.g., `+` followed by nothing, or ` ` followed by nothing). Never emit a
  zero-character line inside the diff block.

#### 1.2.2 Internal consistency

The `Diff` block MUST be internally consistent with the `After` block and the
original source code: applying the diff to the original source must produce
the `After` block exactly, character-for-character. If you cannot guarantee
this (e.g., the change is too large or sweeping), say so explicitly and omit
the `Diff` block rather than emitting an incorrect one. The `After` block
remains required.

#### 1.2.3 For visual red / green highlighting

If the user wants color-highlighted diffs, instruct them to use Visual Studio's
native **Apply** or **Preview** action on the returned `After` code block —
that view shows red/green line backgrounds against the live file. The chat
panel itself will not colorize the `Diff` block.

### 1.3 Output completeness

When modifying or generating code, return the **full file or full code block
in its entirety**.

You MUST NOT:
- Truncate output
- Summarize unchanged sections
- Use placeholder comments such as `// existing code here...`,
  `<!-- ... -->`, `/* unchanged */`, or `// ...`

### 1.4 Preserve user-facing text verbatim

Never alter, rephrase, correct, reword, or "improve" any existing user-facing
text content. All copy, labels, headings, body text, error messages, button
text, and alt text must be returned **exactly as provided** — character for
character — including punctuation, capitalization, and whitespace.

---

## 2. Styling Conventions

### 2.1 Font sizing

Always use `em` units for font sizing.
Only deviate when the user explicitly specifies a different unit
(e.g., `px`, `rem`, `vw`, `%`).
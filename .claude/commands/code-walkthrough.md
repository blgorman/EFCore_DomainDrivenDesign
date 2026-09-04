---
description: Apply one course clip's student instructions to this repository exactly as written — creates a walkthrough branch, follows every step verbatim, stops and reports in full on any error instead of fixing it, and leaves the changes uncommitted for review.
argument-hint: Module NN Clip NN
---

# Code Walkthrough

You are applying one clip's student instructions to this repository, exactly as a student following the course would. The person you are working with is a student, not the course author.

Your job is to **execute one clip document literally and report what happened.** You are not reviewing the course, improving it, or getting the student unstuck by being clever. A walkthrough that needed you to think creatively has already failed, and the student needs to know that.

---

## Invocation

The student names a module and a clip, in any of these shapes:

- "use the code-walkthrough skill to implement the student instructions for Module 5, Clip 3"
- "/code-walkthrough Module 4 Clip 5"
- "/code-walkthrough 6 2"

Resolve that to a zero-padded module and clip number: Module 5 Clip 3 becomes module `05`, clip `03`.

If the student names a module but no clip, ask which clip. Do not guess, and do not apply a whole module in one pass — one clip per invocation, always.

---

## Step 1 — Check the working tree is clean

Run `git status --short`.

The whole point of this skill is that the student can see precisely what the clip's instructions changed. Uncommitted edits from earlier work make that diff unreadable.

If the working tree is not clean, **stop and report**. Show what is modified and let the student decide whether to commit, stash, or discard. Do not stash or discard anything yourself.

---

## Step 2 — Create the branch

Create and switch to a branch named for the module and clip:

```
walkthrough-module05-clip03
```

Always `walkthrough-module<NN>-clip<NN>`, zero-padded, lowercase.

```bash
git switch -c walkthrough-module05-clip03
```

If a branch by that name already exists, **stop and report**. The student has run this clip before, and silently reusing or overwriting that branch would destroy their earlier work. Let them choose whether to delete it, rename it, or switch to it.

---

## Step 3 — Open that clip's instructions, and only that clip's

The student-facing clip documents live at:

```
Instructions/Module<NN>/Clip<NN>-<Title>.md
```

Find the one file matching `Instructions/Module<NN>/Clip<NN>-*.md` and read it in full before changing anything.

Coverage as the repository ships: Module 02 has Clips 01-09, Module 03 has Clips 01-08, Module 04 has Clips 01-09, Module 05 has Clips 01-06, Module 06 has Clips 01-08, and Module 07 has Clip 04 only. Module 01 is conceptual and has no clip documents.

If no document matches, **stop and report** which path you looked for. Do not substitute a neighboring clip.

Read **only** that clip's document. Do not read ahead to later clips, do not read earlier clips to "catch up," and do not consult the finished solution anywhere. The student is progressing through concepts in sequence, and a change pulled in from outside this clip's scope corrupts the exercise even when it compiles.

---

## Step 4 — Apply the instructions verbatim

Work through the document's student steps in order, doing exactly what each step says.

**Verbatim means verbatim.** Type what the step says to type, in the file the step names, at the location the step names. If a step says to delete an active line and uncomment the line below it, do that and nothing more.

Rules while applying:

- **A step that needs interpretation has already failed.** If you cannot tell what to type, where to put it, or which of two readings is meant, that is a defect in the document. Stop and report it. Do not pick the reading that seems most likely.
- **Never implement a `//TODO` the clip does not ask you to implement.** Markers for other clips stay exactly as they are.
- **Normalize indentation when uncommenting.** Uncommenting means removing the `//` prefix and restoring the line to the indentation its surrounding code uses, not mechanically stripping two characters.
- **Keep the comment style.** Commented-out code in this repository uses line-by-line `//` prefixes. Never introduce `/* */` block comments.
- **Build when the document says to build**, and always build once at the end:

  ```bash
  dotnet build ConsoleAppProject.slnx
  ```

- **Run whatever the document tells you to run** — a demo through the menu, or a filtered test run such as `dotnet test --filter Module=4`. Report the real result. Never describe a build or a test as passing or failing without having run it; if you did not run something, say "unverified" rather than guessing.

Two environment notes that are not walkthrough failures:

- **`MSB3027` / `MSB3021` file-lock errors are not compile errors.** They mean a console instance is still running and holding the output file. Close it and build again. Only an actual `error CS####` is a compile failure.
- **On Windows ARM64, LocalDB loads only in x64 processes.** A connection failure with error 193 or SNI error 56 and a `ClientConnectionId` of all zeros is an architecture mismatch, not a missing server. Run database-touching commands as x64, for example `dotnet run -a x64 --project ConsoleAppProject`.

---

## Step 5 — On any error, FULL STOP

**This is the most important rule in this skill.**

The moment anything does not work — a compile error, a failing test the document said would pass, a demo that throws an exception, a step whose target text is not in the file, a step you cannot carry out as written — **stop immediately.**

Do not fix it. Do not work around it. Do not adjust the code so the walkthrough can continue. Do not proceed to the next step. Leave the repository exactly as it stands at the moment of failure, so the student and the author can see the real state.

You should not hit an error here. The author spent two days aligning these documents against the code so that every clip applies cleanly. That is exactly why a failure matters enough to stop for: it is a genuine defect worth reporting, not a bump to smooth over.

Then report, in this shape:

**Stopped at Clip _N_ Step _M_.**

1. **What the error is.** The exact message, and the full repository-relative path with the line number, for example `OrderManagement.Infrastructure/Repositories/OrderRepository.cs:34`. Quote the compiler or test output rather than paraphrasing it.
2. **Why it likely happened**, if you can tell. Name the concrete cause: the document says to uncomment a block that references a member that does not exist yet, or a prerequisite from an earlier clip was never applied, or the document's target line does not match the file's current text. If you genuinely cannot tell, say so plainly instead of speculating.
3. **What a fix would take.** Describe the change — which file, which line, what would need to be different in the document or in the starter code. Describe it only. **Do not apply it.**

Always refer to steps as "Clip _N_ Step _M_", never a bare step number, because a bare number reads as a clip number.

Report the one blocking failure. Do not append a list of other things you noticed, and do not offer a menu of fix options.

---

## Step 6 — Report complete, and do not commit

When every step in the document has been applied and the final build and any specified tests have run:

- Run `git status --short` and `git diff --stat`, and show both.
- State what changed: which files, and what the clip accomplished in one or two sentences.
- State the verified results — the build outcome and any test counts you actually observed.

**Never commit, and never push.** The entire value of this walkthrough is the student reading the uncommitted diff to see exactly what the student documents produced. Leave the changes in the working tree on the walkthrough branch and stop there.

If the student later asks you to commit, that is a separate instruction and it is theirs to give.

---

## Scope

Answer questions about what you did. Change nothing that the clip document did not ask for. If the student asks a question, answer it and edit nothing — only an instruction to change something authorizes a change.

# Aloe Programming Language

> A small, strongly-typed, “pipe-oriented” language being designed together with a generative AI.  
> License: MIT

Aloe is an **experimental programming language** with static typing, a simple syntax,  
and a stack-based VM. It is especially designed to treat **I/O and data transformation**  
via **pipes** and **filters** as first-class features.

Current focus:

- A **language specification** that is readable and explicitly written down
- An AloeVM bytecode format (`AloeBC`) that is **easy to map to WebAssembly**
- A **pipe/filter model** that feels good for text processing, REST calls, and streaming

> ⚠️ Aloe is experimental and not intended for production use.  
> The syntax, VM, and toolchain may still change in backward-incompatible ways.

---

## Documentation

- Aloe language specification (HTML)  
  https://stk2k.github.io/Aloe-lang/Documents/en/Aloe-lang-spec-en.html
- AloeVM specification (HTML)  
  https://stk2k.github.io/Aloe-lang/Documents/en/Aloe-vm-spec-en.html
- AloeVM opcode specification (HTML)  
  https://stk2k.github.io/Aloe-lang/Documents/en/Aloe-vm-opcode-en.html

---

## Features (Design Goals)

### Static Typing + Type Inference

```aloe
var n = 42;      // inferred as int
var pi = 3.14;   // inferred as float
let name: string = "Aloe";
```

- Local type inference with `var`
- Explicit type annotations with `let`

### Clear Distinction Between Value Types and Reference Types

- `struct` … value type (copied by value)
- `class` … reference type (heap-allocated)
- `delete obj;` … dedicated syntax that sets the reference to `null`  
  (`obj = null;` is not allowed in source code)

### Simple, Explicit Control Structures

- `;` is required at the end of every statement
- Indentation has no meaning (unlike Python-style syntax)
- Exception handling with `try / catch / finally`

---

## I/O and Data Flow with Pipes and Filters

You can describe data pipelines at the language level.

```aloe
main(args: string[]) {
    let lines: pipe<string> = pipe<string>.create();

    stdin
      | filter(utf8)       // byte -> string
      | filter(lineSplit)  // string -> string (one line each)
      | lines;

    foreach (line in lines) {
        if (line == "") { break; }
        print("LINE: " + line);
    }

    _ = 0; // exit code
}
```

### Defining a Filter

```aloe
filter lineSplit {
    in:  pipe<string>;
    out: pipe<string>;

    bound(input, output) {
        foreach (chunk in input) {
            let lines = chunk.split("\n");
            foreach (line in lines) {
                output.write(line);
            }
        }
        output.close();
    }
}
```

- Input/output types are declared with `in` / `out`
- `bound(input, output)` implements the actual transformation
- In a pipeline you use it as `filter(lineSplit)`

---

## Mix-in Style with `trait` + `with`

```aloe
trait Printable {
    method printSelf(): void {
        print(this.toString());
    }
}

class User {
    field name: string;
    method toString(): string {
        return "User(" + name + ")";
    }
}

main(args: string[]) {
    var u = new User();
    u.name = "alice";

    var p = u with Printable;
    p.printSelf();   // "User(alice)"

    _ = 0;
}
```

- A trait is a reusable unit that can define methods
- `with` attaches traits to an instance (mixin-like semantics)

---

## Overview of AloeVM & AloeBC

- Assumes a stack-based VM
- Compact bytecode format `AloeBC`
  - Starts with the magic `"ALOEBC"` and version bytes (e.g. Major/Minor/Build)
- Instruction set is designed to map **as closely 1:1 as possible to WebAssembly**

### WASM-Friendly Design

- AloeVM instructions are designed so that they can be mapped straightforwardly to WebAssembly opcodes
- Long-term ideal:

  - Development flow:  
    `Aloe source → WASM → Execution environment (browser / server / etc.)`
  - The `AloeVM` interpreter is mainly for debugging and non-WASM environments

---

## Example: Simple REST-Like Pipeline

This is an imagined example that fetches one user record using an HTTP client  
and a JSON filter (the concrete standard library APIs are still under design).

```aloe
struct UserInfo {
    field id:    int;
    field name:  string;
    field email: string;
}

main(args: string[]) {
    let request:  pipe<byte>     = pipe<byte>.create();
    let response: pipe<byte>     = pipe<byte>.create();
    let users:    pipe<UserInfo> = pipe<UserInfo>.create();

    // Pseudo code: httpClient performs an HTTP call using request/response
    httpClient("GET", "https://api.example.com/users/1", request, response);

    response
        | filter(utf8)
        | filter(json<UserInfo>)
        | users;

    // Imaginary method that pulls the first element from the current pipe
    let u = users.next();

    if (u is UserInfo) {
        print("User: " + u.name + " <" + u.email + ">");
    }

    _ = 0;
}
```

Note: details such as `httpClient`, options for `json<T>`, and the standard filter/library  
APIs are being designed in a separate document.

---

## Project Layout (Draft)

A possible repository structure (subject to change):

```text
.
├─ docs/
│  ├─ aloe-lang-spec-ja.md      # Aloe language spec (Japanese)
│  ├─ aloe-lang-spec-en.md      # Aloe language spec (English)
│  ├─ aloe-vm-spec-en.md        # AloeVM / AloeBC / GC / WASM mapping spec
│  └─ filters-standard-lib.md   # Standard filter library spec
├─ src/
│  ├─ compiler/                 # Aloe → AloeBC compiler (C#, WIP)
│  ├─ vm/                       # Native AloeVM interpreter (C#, WIP)
│  └─ aloe2wasm/                # Aloe → WASM backend (WIP)
├─ examples/
│  ├─ hello-world.aloe
│  ├─ pipes/                    # pipe + filter samples
│  └─ web/                      # Simple REST / Web-like samples
├─ tests/
│  └─ ...
├─ LICENSE
└─ README.md
```

At the moment, the specs under `docs/` are the “source of truth” for the language.

---

## Status

- ✅ **Language design**  
  Drafts exist for the type system, control structures, pipes/filters, traits, exceptions, and many other details.
- ✅ **VM design**  
  Drafts exist for the stack model, `AloeBC` bytecode, GC strategy, and WASM mapping.
- ⏳ **Implementation**  
  Compiler, VM, and WASM backend are under development or still at the idea stage.
- ⏳ **Tooling**  
  REPL, formatter, LSP, package manager, etc. are future work.

Issues, PRs, and discussions from anyone interested in experimental language/runtime design are very welcome.

---

## How to Contribute

**Implementation**

- Prototypes for parts of the compiler (parser, type checker, code generation, etc.)
- A simple AloeVM interpreter
- A prototype `aloe2wasm` backend for a minimal core subset

**Spec Improvements**

- Find ambiguous parts and edge cases, and propose clarifications
- Suggest alternative syntax or semantics (ideally with concrete examples)

**Samples & Tests**

- Text tools that make good use of pipes/filters
- Simple REST / Web-style flows
- Pattern collections using traits + `with`

If you plan to propose a large design change, it helps a lot to open an Issue or Discussion  
before sending a PR, so we can keep the overall language design consistent.

---

## Roadmap (Rough)

### Language

- “Freeze” the syntax and type system for a 0.x series
- Stabilize the standard filter library spec
- Flesh out the details of collections and standard types

### VM

- Finalize the instruction set (opcodes) and AloeBC binary format
- Implement a reference C# VM including a simple GC

### WASM

- Implement mapping from Aloe to WASM for a “core subset”
- Prototype direct WASM execution without going through an interpreter

### Tooling

- CLI tools (e.g. `aloec`, `aloevm`)
- Formatter / syntax highlighting
- A simple package management mechanism (can be very minimal at first)

---

## License

Aloe is planned to be released under the **MIT License**.  
See the `LICENSE` file in the repository for details.

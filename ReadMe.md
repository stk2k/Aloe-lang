````markdown
# Aloe Programming Language

> A small, strongly-typed, pipe-friendly language designed together with an LLM.  
> License: MIT

Aloe is an experimental, statically-typed language with a simple syntax, a stack-based VM, and first-class support for **pipes** and **filters** for I/O and data transformation.

The project currently focuses on:

- A **language spec** that’s easy to read and reason about.
- An **AloeVM** bytecode format (`AloeBC`) designed to map cleanly to **WebAssembly**.
- A composable **pipe/filter** model that makes tasks like text processing, REST calls, and streaming data feel natural.

> ⚠️ Aloe is experimental and not production-ready yet.  
> Breaking changes to the syntax, VM and tooling are expected.

---

## Features (Design)

- **Static typing with type inference**

  ```aloe
  var n = 42;      // inferred as int
  var pi = 3.14;   // inferred as float
  let name: string = "Aloe";
````

* **Distinct value / reference types**

  * `struct` = value type (copied by value)
  * `class` = reference type (heap allocated)
  * `delete obj;` sets a reference to `null` (no explicit `= null` in source)

* **Simple, explicit control flow**

  * All statements end with `;`
  * No significant indentation
  * `try / catch / finally` for exceptions

* **Pipes and filters for I/O and data flow**

  Modeled directly in the language:

  ```aloe
  main(args: string[]) {
      let lines: pipe<string> = pipe<string>.create();

      stdin
        | filter(utf8)       // bytes -> string
        | filter(lineSplit)  // string -> string (per line)
        | lines;

      foreach (line in lines) {
          if (line == "") { break; }
          print("LINE: " + line);
      }

      _ = 0; // exit code
  }
  ```

* **Filter definitions**

  ```aloe
  filter lineSplit {
      in:  pipe<string>;
      out: pipe<string>;

      bound(input, output) {
          foreach (chunk in input) {
              let parts = chunk.split("\n");
              foreach (line in parts) {
                  output.write(line);
              }
          }
          output.close();
      }
  }
  ```

* **First-class traits + `with` sugar**

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

* **AloeVM & AloeBC**

  * Stack-based VM with a compact bytecode (`AloeBC`).
  * `AloeBC` files start with magic `ALOEBC` and version bytes.
  * Instruction set designed to mirror WebAssembly as much as possible.

* **WASM-friendly**

  * Planned 1:1-ish mapping from AloeVM instructions to WASM opcodes.
  * Long-term goal:

    * Dev workflow: Aloe → WASM → host (browser, server, etc.)
    * `AloeVM` interpreter optional, mainly for debugging and non-WASM environments.

---

## Example: Simple REST-style Pipeline

Conceptual example of using a pipe-based HTTP client + JSON filter to fetch a user:

```aloe
struct UserInfo {
    field id:   int;
    field name: string;
    field email: string;
}

main(args: string[]) {
    let request: pipe<byte>  = pipe<byte>.create();
    let response: pipe<byte> = pipe<byte>.create();
    let users: pipe<UserInfo> = pipe<UserInfo>.create();

    // pseudo: httpClient is a built-in or library function
    // that sends "request" and fills "response"
    httpClient("GET", "https://api.example.com/users/1", request, response);

    response
        | filter(utf8)
        | filter(json<UserInfo>)
        | users;

    // Get the first (current) value from the pipe
    let u = users.next();  // or users.take(1) depending on final naming

    if (u is UserInfo) {
        print("User: " + u.name + " <" + u.email + ">");
    }

    _ = 0;
}
```

> Note: concrete standard library APIs (e.g. `httpClient`, `json<T>` options, etc.) are still under design.
> The example shows the intended *style* of Aloe programs.

---

## Project Structure (Planned)

A possible structure for the repository (names may change):

```text
.
├─ docs/
│  ├─ aloe-lang-spec-ja.md      # Aloe language spec (Japanese)
│  ├─ aloe-lang-spec-en.md      # Aloe language spec (English)
│  ├─ aloe-vm-spec-en.md        # AloeVM + AloeBC + GC + WASM mapping
│  └─ filters-standard-lib.md   # Standard filter library spec
├─ src/
│  ├─ compiler/                 # Aloe → AloeBC compiler (C#, WIP)
│  ├─ vm/                       # Native AloeVM interpreter (C#, WIP)
│  └─ aloe2wasm/                # Aloe → WASM backend (WIP)
├─ examples/
│  ├─ hello-world.aloe
│  ├─ pipes/                    # pipe + filter samples
│  └─ web/                      # simple REST / web-style examples
├─ tests/
│  └─ ...
├─ LICENSE
└─ README.md
```

At the moment, the primary “source of truth” is the specification in `docs/`.

---

## Status

* ✅ Language design: **draft** but fairly detailed (types, control flow, pipes/filters, traits, exceptions).
* ✅ VM design: **draft** (stack model, bytecode format, GC policy, WASM mapping).
* ⏳ Implementation: compiler / VM / WASM backend are **work in progress**.
* ⏳ Tooling: REPL, formatter, LSP, package manager, etc. are **future work**.

If you are interested in experimental language/runtime design, PRs and discussions are welcome.

---

## Building (When Implementation Exists)

> This section is forward-looking. Adjust commands to your actual tooling/paths once the implementation is in place.

### Prerequisites

* .NET 8 SDK (for the reference C# implementation)
* A recent C# compiler / IDE (optional but recommended)
* For WASM backend:

  * `dotnet` with WASM support or
  * an external toolchain depending on implementation

### Example

```bash
# Clone
git clone https://github.com/your-org/aloe-lang.git
cd aloe-lang

# Build compiler + VM
dotnet build src/compiler
dotnet build src/vm

# Run tests
dotnet test
```

---

## Contributing

Aloe is intentionally experimental and opinionated. Contributions that help clarify, simplify, or strengthen the design are very welcome.

Ways to contribute:

* Try to implement:

  * A small part of the compiler (parser / type checker / codegen).
  * A toy AloeVM interpreter.
  * A prototype `aloe2wasm` backend.
* Improve / correct the specs:

  * Clarify edge cases.
  * Propose alternative syntax or semantics (with concrete examples).
* Add examples and tests:

  * Pipe-heavy text tools.
  * Simple web-style flows (REST, JSON pipelines).
  * Trait + `with` usage patterns.

Please open an issue or discussion before large design changes so we can keep the language coherent.

---

## Roadmap (High-level)

* Language:

  * Freeze a 0.x syntax & type system.
  * Flesh out standard filter library spec.
  * Define core collections & standard types in more detail.
* VM:

  * Finalize instruction set and AloeBC format tables.
  * Implement a reference C# VM with simple GC.
* WASM:

  * Implement Aloe → WASM mapping for a “core subset”.
  * Prototype direct WASM builds without the interpreter.
* Tooling:

  * Basic CLI (`aloec`, `aloevm`).
  * Formatter / syntax highlighter.
  * Basic package story (even a very simple one).

---

## License

Aloe is released under the **MIT License**.

See the [`LICENSE`](./LICENSE) file for full details.

```
::contentReference[oaicite:0]{index=0}
```

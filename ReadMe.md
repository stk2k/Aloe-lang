# Aloe Programming Language

> A small, strongly-typed, **pipe-oriented** language designed together with generative AI.  
> License: MIT

Aloe is an experimental programming language with:

- Static typing and type inference
- A simple, explicit syntax
- A stack-based virtual machine (AloeVM)
- A design that maps well to **WebAssembly**

A distinctive feature of Aloe is that **I/O and data transformation are modeled around `pipe` and `filter`** as first-class concepts.

> $26A0$FE0F Aloe is **highly experimental** and not intended for production use yet.  
> The syntax, VM, and toolchain may change in backward-incompatible ways.

---

## Goals & Philosophy

- **Clarity over cleverness**  
  The language is meant to be easy to read and to implement, especially for people who like reading language/VM specs.

- **Pipe-first design**  
  Streaming text, JSON, and structured data via `pipe<T>` and `filter(...)` should feel natural and ergonomic.

- **VM + WASM orientation**  
  AloeVM bytecode (`AloeBC`) is designed so that instructions can map 1:1 (or very closely) to WebAssembly opcodes.

- **Spec-first**  
  The language and VM are documented in detail before (or while) being implemented. The specs are the source of truth.

---

## Language at a Glance

### Static typing with inference

```aloe
var n  = 42;      // inferred as int
var pi = 3.14;    // inferred as float
let name: string = "Aloe";
```

- `var` $2013 local type inference
- `let` $2013 explicit type annotation
- No `any` / dynamic type

### Value types vs reference types

- `struct` $2013 value type (copied by value)
- `class`  $2013 reference type (allocated on the heap)
- `string` $2013 reference type internally, but with special syntax
- `delete obj;` $2013 sets a reference to `null` (you cannot write `obj = null;` directly)

Example:

```aloe
class User {
    field id:   int;
    field name: string;

    construct(id: int, name: string) {
        this.id   = id;
        this.name = name;
    }
}
```

### Explicit control flow

- Semicolons (`;`) are mandatory at the end of every statement
- Indentation has no semantic meaning (unlike Python)
- Exceptions with `try / catch / finally`

```aloe
try {
    doSomething();
}
catch (e) {
    print("error: " + e.toString());
}
finally {
    cleanup();
}
```

---

## Pipes & Filters

Aloe treats **streams of values** as first-class citizens via `pipe<T>`.  
Filters transform these streams and are composed with a pipeline syntax.

### Basic example: reading from stdin

```aloe
main(args: string[]) {
    let lines: pipe<string> = pipe<string>.create();

    stdin
        | filter(utf8)       // byte -> string
        | filter(lineSplit)  // string -> string (per line)
        | lines;

    foreach (line in lines) {
        if (line == "") { break; }
        print("LINE: " + line);
    }

    _ = 0; // exit code
}
```

- `stdin` is a built-in `pipe<byte>`
- `filter(utf8)` encodes/decodes between `byte` and `string` depending on the input type
- `filter(lineSplit)` splits into line-by-line chunks
- The final `lines` pipe is consumed with `foreach`

### Defining a filter

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

- `in` / `out` declare the input/output types of the filter
- `bound(input, output)` contains the processing logic
- Filters are VM values with special roles for `in` and `out`

### Pipe methods: next / take / peek

For convenience, pipes provide a few built-in methods (semantics still evolving in the spec):

- `next()` $2013 get the next element if available (or a sentinel / exception on EOF)
- `take(n)` $2013 a new pipe or collection containing up to `n` elements
- `peek()` $2013 look at the next value without consuming it (implementation-dependent)

Example:

```aloe
let users: pipe<UserInfo> = getUsersPipe();

let firstUser = users.next();
if (firstUser is UserInfo) {
    print("First user: " + firstUser.name);
}
```

---

## Traits and `with` syntax

Traits are reusable groups of methods that can be attached to existing types.

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
    p.printSelf();  // "User(alice)"

    _ = 0;
}
```

- Traits provide an alternative to multiple inheritance
- `with` attaches trait behavior to an instance

---

## Exception model (high level)

Standard exception types are arranged in a simple hierarchy (spec in progress). Examples:

- `Exception`
  - `SystemException`
    - `NullReferenceException`
    - `IndexOutOfBoundsException`
    - `OverflowException` (covers underflow as well)
    - `ZeroDivisionException` (thrown on division by `0` or `0.0`)
    - `InvalidOperationException`
    - `NotImplementedException`
    - `TimeoutException`
    - `ArgumentException`
      - `ArgumentNullException`
      - `ArgumentOutOfRangeException`
    - `FormatException`
  - `IOException`
    - `FileNotFoundException`
    - `EndOfStreamException`

Many details (e.g., exact mapping to VM codes) are still being refined.

---

## AloeVM & AloeBC

AloeVM is a stack-based virtual machine designed to be:

- Small and relatively easy to implement
- Friendly to WebAssembly code generation
- Backed by an explicit bytecode format: **AloeBC**

### AloeBC

- Binary format with a magic header, e.g.:

  - Magic: `ALOEBC`
  - Version: 3 bytes (Major, Minor, Build)

- Contains:
  - Constant pool
  - Type information
  - Function definitions
  - Bytecode instructions

The exact instruction set is being designed so that **each AloeVM opcode has a straightforward mapping to a WASM opcode or sequence**, making an `aloe2wasm` backend natural.

### WASM strategy

Long-term vision:

- **Development**:  
  `Aloe source` → `AloeBC` → `WASM` (via aloe2wasm) → any WASM runtime
- **Runtime modes**:
  - Native AloeVM interpreter (C#, for example) for:
    - Debugging
    - Non-WASM platforms
  - Direct WASM compilation/execution for:
    - Browsers
    - Server runtimes that support WASM

Garbage collection for the VM is specified at a high level (mark/sweep$2013like strategies, block management, etc.), while WASM-native GC is delegated to the WASM environment where available.

---

## Ecosystem (Planned / In Progress)

The project is very early. A possible repository layout:

```text
.
├─ docs/
│  ├─ aloe-lang-spec-en.html   # Language spec (English, HTML)
│  ├─ aloe-lang-spec-ja.html   # Language spec (Japanese, HTML)
│  ├─ aloe-vm-spec-en.html     # VM / AloeBC / GC / WASM mapping
│  └─ filters-standard-lib.md  # Standard filter library
├─ src/
│  ├─ compiler/                # Aloe → AloeBC compiler (C#, WIP)
│  ├─ vm/                      # AloeVM interpreter (C#, WIP)
│  └─ aloe2wasm/               # Aloe → WASM backend (WIP)
├─ examples/
│  ├─ hello-world.aloe
│  ├─ pipes/
│  └─ web/
├─ tests/
├─ LICENSE
└─ README.md
```

Right now, **the specs in `docs/` are the authoritative reference**.

---

## Contributing

This project is exploratory and welcomes:

- Feedback on language and VM design
- Prototype implementations (parsers, type checkers, VMs)
- Examples that use `pipe`/`filter` in interesting ways
- Discussions about WASM mapping strategies

Because Aloe is still evolving, large changes to the language or VM should ideally be discussed via an issue or discussion thread before being implemented in a PR.

Some ideas for contributions:

- Implement a minimal Aloe subset compiler in C#
- Implement a toy AloeVM for that subset
- Experiment with an `aloe2wasm` prototype that maps a small subset to WASM

---

## Roadmap (Very Rough)

### Language

- Stabilize a 0.x syntax and type system
- Finalize `pipe` / `filter` semantics
- Specify standard filters (UTF-8, JSON, line splitting, etc.)

### VM

- Finalize the AloeVM instruction set
- Freeze AloeBC binary format
- Provide a reference C# implementation with a simple GC

### WASM

- Design a clear mapping from AloeVM ops to WASM
- Implement `aloe2wasm` for a core subset
- Explore linking Aloe-generated WASM with other WASM libraries

### Tooling

- CLI tools: `aloec` (compiler), `aloevm` (VM runner)
- Basic formatter and syntax highlighting
- Simple package distribution story (even a minimal one)

---

## License

Aloe is released under the **MIT License**.

See the `LICENSE` file for details.

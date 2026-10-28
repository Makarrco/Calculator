# 🧮 WPF Calculator
 
![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/UI-WPF-0078D4?logo=windows&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
 
A Windows 11–style calculator built with **WPF and C#**. It handles integer and decimal arithmetic, chained operations, memory, and full keyboard control. No external packages, no NuGet, no magic. Just clean code.
 
---
 
## ✨ Features
 
- **Basic operations:** `+` `−` `×` `÷` on integers and decimals
- **Chained calculations:** `2 + 3 × 4` evaluates step by step, just like Windows Calculator
- **Repeat equals:** `5 + 3 = = =` gives `8 → 11 → 14`
- **Extra functions:** `%`, `1/x`, `x²`, `√x`, `+/−`
- **Memory:** `MC` `MR` `M+` `M−` `MS`
- **Input validation:** blocks leading zeros (`007`), allows only one decimal point, caps numbers at 16 digits
- **Precise math:** uses `decimal`, so `0.1 + 0.2 = 0.3` (not `0.30000000000000004`)
- **Error handling:** division by zero, square root of a negative number, overflow
- **Read-only display:** two fields, one for the expression history and one for the current number, both filled only by buttons
- **Auto-fit display:** long numbers shrink to fit instead of being cut off
---
 
## 🚀 Getting started
 
**Requirements:** Windows, Visual Studio 2022 with the *.NET desktop development* workload, .NET 8 SDK
 
1. Unzip the project
2. Open `Calculator.sln`
3. Press **F5**
> Using an older .NET? Change `net8.0-windows` in `Calculator.csproj` to your version (for example `net6.0-windows`).
 
---
 
## ⌨️ Controls
 
| Button | Key | Action |
|:------:|:---:|--------|
| `0`–`9` | `0`–`9` | Append a digit |
| `.` | `.` or `,` | Add a decimal point |
| `+` `−` `×` `÷` | `+` `-` `*` `/` | Arithmetic operation |
| `=` | `Enter` or `=` | Calculate |
| `⌫` | `Backspace` | Delete the last character |
| `CE` | `Delete` | Clear the current number |
| `C` | `Esc` | Clear everything |
| `+/−` | `F9` | Change the sign |
| `%` | `%` | Percent |
| `¹/ₓ` | `R` | Reciprocal |
| `x²` | `Q` | Square |
| `²√x` | `@` | Square root |
 
---
 
## 🗂️ Project structure
 
```
Calculator/
├── Calculator.sln
├── README.md
└── Calculator/
    ├── Calculator.csproj      # .NET 8 WPF project
    ├── App.xaml(.cs)          # application entry point
    ├── AssemblyInfo.cs
    ├── MainWindow.xaml        # UI: layout, styles, buttons
    └── MainWindow.xaml.cs     # calculator logic
```
 
---
 
## 🧠 How it works
 
The calculator is a small **state machine**. Instead of parsing a whole expression string, it tracks a few fields:
 
| Field | Purpose |
|-------|---------|
| `current` | The number on the display, kept as a string so input stays exact |
| `accumulator` | The result of the previous operation |
| `pendingOp` | The operator waiting for its second operand |
| `newEntry` | Whether the next digit starts a new number |
| `lastOp` / `lastOperand` | Used to repeat the last operation when `=` is pressed again |
 
Each button changes the state and refreshes both display fields. That's the same approach the real Windows Calculator uses in standard mode.
 
---
 
## 📸 Example
 
```
  12 × 3 = 36
```
 
---
 
<sub>Made as a homework assignment on WPF fundamentals.</sub>

# selenium-specflow-patterns.context.md

## Purpose
Reusable patterns for C# + Selenium + SpecFlow agents.
Learned from: SauceDemo checkout implementation (2026-03-30).
Apply to: any modern web app — React, Vue, Angular, or vanilla HTML.

---

## Pattern 1 — React-controlled inputs

### Problem
`SendKeys` types into the DOM input but does NOT update React's internal state.
React reads its own state on form submission → validation ignores typed values.

**Symptom:** "First Name is required" even after `SendKeys("John")`.
**Root cause:** React's synthetic `onChange` handler is not triggered by Selenium's native key events in some configurations.

### Solution
Use the native HTMLInputElement value setter + dispatch `input` event:

```csharp
private void SetField(string css, string value)
{
    var field = WaitFor(css);
    _js.ExecuteScript(
        "var s = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;" +
        "s.call(arguments[0], arguments[1]);" +
        "arguments[0].dispatchEvent(new Event('input', {bubbles:true}));",
        field, value);
}
```

### When to use
- SPA frameworks: React, Vue, Angular (any controlled input)
- Detectable by: form rejects values typed via SendKeys in validation

### When NOT needed
- Server-rendered HTML forms (Django, Rails, MVC views)
- Login pages that pre-date the SPA framework adoption (e.g., SauceDemo login works with plain SendKeys)

### Diagnostic test
If `SendKeys` works on page A but not on page B of the same site → page B uses React-controlled inputs.

---

## Pattern 2 — JS click for SPA buttons

### Problem
Native `.Click()` can be intercepted or ignored by React/Vue's synthetic event system.
Buttons may appear clicked but no action occurs.

**Symptom:** Click runs but form doesn't submit / navigation doesn't happen.

### Solution
```csharp
_js.ExecuteScript("arguments[0].click();", WaitFor("[data-test='submit']"));
```

### Exception
Native `.Click()` DOES work for some buttons (e.g., SauceDemo Continue button works with native click
when the form has NO values — empty form validation fires normally).
**Rule:** use JS click when a button does not respond to native click after field entry.

---

## Pattern 3 — WaitFor helper (always use in Page Objects)

### Problem
`_driver.FindElement(...)` after navigation can find a stale element or throw `NoSuchElementException`
if the page hasn't fully rendered.

### Solution
Define a single helper in every Page Object:
```csharp
private IWebElement WaitFor(string css) =>
    _wait.Until(d => d.FindElement(By.CssSelector(css)));
```

Use it for EVERY element interaction. Never call `_driver.FindElement` directly.

### Timing guarantee
`_wait.Until` retries every 500ms until element found OR timeout.
The element IS in DOM when the method returns — but may not be visible.
For visibility: add `.Displayed` check (see Pattern 4).

---

## Pattern 4 — Wait for visibility vs presence

### Presence (element exists in DOM)
```csharp
private IWebElement WaitFor(string css) =>
    _wait.Until(d => d.FindElement(By.CssSelector(css)));
```
Use for: form inputs, buttons, links.

### Visibility (element in DOM AND displayed)
```csharp
private IWebElement WaitVisible(string css) =>
    _wait.Until(d => {
        try {
            var el = d.FindElement(By.CssSelector(css));
            return el.Displayed ? el : null;
        } catch (NoSuchElementException) { return null; }
    })!;
```
Use for: modals, tooltips, overlays, elements that render hidden first.

**Note:** For SauceDemo-style React forms, `WaitFor` (presence) is sufficient — elements are visible when present.

---

## Pattern 5 — Wait after state-changing actions

### Problem
After clicking Remove/Delete/Close, assertions run before the DOM updates.
`IsCartBadgeVisible()` returns `true` because the badge hasn't disappeared yet.

### Solution
Always wait for the RESULT state, not just after the trigger:
```csharp
// WRONG — no wait
btn.Click();
return badge.Displayed; // might still be true

// CORRECT — wait for state change
int countBefore = _driver.FindElements(By.CssSelector(".badge")).Count;
btn.Click();
_wait.Until(d => d.FindElements(By.CssSelector(".badge")).Count < countBefore);
```

### General formula
1. Capture state BEFORE action
2. Perform action
3. Wait until state AFTER action matches expected

---

## Pattern 6 — SpecFlow Background keyword alignment

### Problem
SpecFlow `And` inherits the keyword type of the PREVIOUS step.
Step definitions decorated with `[Given]`, `[When]`, `[Then]` are NOT interchangeable in strict mode.
A step defined as `[When]` will NOT match `And` that inherited `Given` context.

### Symptom
```
And I navigate to the cart page
→ No matching step definition found
```
Even though `[When(@"I navigate to the cart page")]` exists.

### Solution
Explicitly change keyword context in Background when switching from Given to When:
```gherkin
Background:
  Given I am logged in as standard user       ← Given context
  And I add first product to cart             ← Given ✓ (inherits)
  When I navigate to the cart page            ← Resets to When context
  And I click checkout                        ← When ✓ (inherits)
```

### Rule
- Setup steps (data preparation) → `Given` + `[Given]`
- Navigation and interactions → `When` + `[When]`
- Don't mix — or explicitly reset with a keyword before the `And` chain

---

## Pattern 7 — Page Object wait strategy

### Standard template for any Page Object
```csharp
public class MyPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private readonly IJavaScriptExecutor _js;

    // Always use WaitFor — never _driver.FindElement directly
    private IWebElement WaitFor(string css) =>
        _wait.Until(d => d.FindElement(By.CssSelector(css)));

    public MyPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(ConfigManager.TimeoutSeconds));
        _js = (IJavaScriptExecutor)driver;
    }

    // For React/Vue/Angular inputs → use JS setter
    private void SetField(string css, string value)
    {
        var field = WaitFor(css);
        _js.ExecuteScript(
            "var s = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;" +
            "s.call(arguments[0], arguments[1]);" +
            "arguments[0].dispatchEvent(new Event('input', {bubbles:true}));",
            field, value);
    }

    // For plain HTML inputs → SendKeys is fine
    private void SetFieldPlain(string css, string value)
    {
        WaitFor(css).SendKeys(value);
    }

    // For buttons → JS click preferred in SPAs
    private void JsClick(string css) =>
        _js.ExecuteScript("arguments[0].click();", WaitFor(css));

    // Navigation assertions — always wait
    public string GetCurrentUrl() => _driver.Url;
    public void WaitForUrl(string partialUrl) =>
        _wait.Until(d => d.Url.Contains(partialUrl));
}
```

---

## Decision tree — which input method to use?

```
Is the site a SPA (React/Vue/Angular)?
├─ YES → Use SetField (JS native setter + input event)
│        THEN JS click for form submission
└─ NO  → Use SendKeys directly
         THEN native .Click() for submission

Did form validation fail after filling correctly?
├─ YES → Switch from SendKeys to JS native setter
└─ NO  → Keep as is

Did button click not trigger action?
├─ YES → Switch from native .Click() to JS click
└─ NO  → Keep native .Click()
```

---

## Context detection — how to know if a site uses React

1. Open DevTools → React DevTools extension shows component tree
2. Check `window.__REACT_DEVTOOLS_GLOBAL_HOOK__` in console
3. Look for `data-reactroot` attribute in HTML
4. Form inputs have no `name` attribute but have `data-testid` / `data-test` → likely React

SauceDemo is React: `https://www.saucedemo.com` has React-controlled inputs on checkout, NOT on login (login uses simple form submission).

---

## GUIDO Scale Tags
- @guido:L4 — optimized standards, cross-cutting pattern library, agent-consumable

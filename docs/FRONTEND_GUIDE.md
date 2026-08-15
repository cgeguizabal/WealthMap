# WealthMap — Frontend Guide

This document exists to teach. It explains the Vue client the same way
[PROJECT_GUIDE.md](PROJECT_GUIDE.md) explains the backend: what each concept is, what problem it
solves, where this codebase uses it, and why each decision went the way it did. Where a choice had a
real alternative, the alternative is named and the trade-off stated.

Read it once top to bottom. After that it works as a reference.

---

## Table of contents

1. [What the client is](#1-what-the-client-is)
2. [Vue fundamentals, in this codebase](#2-vue-fundamentals-in-this-codebase)
3. [Pinia](#3-pinia)
4. [Vue Router](#4-vue-router)
5. [The architecture](#5-the-architecture)
6. [The API integration](#6-the-api-integration)
7. [The design system](#7-the-design-system)
8. [The feature modules](#8-the-feature-modules)
9. [Language](#9-language)
10. [Known limitations](#10-known-limitations)
11. [Glossary](#11-glossary)

---

## 1. What the client is

A Vue 3 single-page app against the WealthMap API. No UI library — every component is hand-built,
deliberately, both to exercise Vue properly and to hit a specific visual language.

| | |
|---|---|
| Framework | Vue 3, Composition API, `<script setup>` everywhere |
| Build | Vite |
| Language | JavaScript (no TypeScript) |
| Styling | SCSS + CSS custom properties, one stylesheet per component (§7.6) |
| Routing | Vue Router, lazy-loaded routes, protected by default |
| State | Pinia (5 stores), composables for everything else |
| HTTP | Axios with two interceptors |
| Animation | motion-v, imported per component |
| Offline | vite-plugin-pwa |

**18** base components, **13** composables, **5** stores, **18** API modules, **20** routes,
**82** component stylesheets, **755** translation keys per language.

---

## 2. Vue fundamentals, in this codebase

### 2.1 The Composition API and `<script setup>`

The old Options API organised a component by *kind of thing*: all data in `data()`, all derived
values in `computed`, all methods in `methods`. That reads fine at 30 lines. At 300 lines, a single
feature — say "the drawer" — is smeared across four separate blocks, and you scroll constantly to
follow one idea.

The Composition API organises by *concern*. Everything about the drawer sits together:

```js
// components/layout/AppShell.vue
const drawerOpen = ref(false)
const collapsed = ref(localStorage.getItem(STORAGE_KEY) === 'true')

function toggleSidebar() { … }

watch(collapsed, (value) => localStorage.setItem(STORAGE_KEY, String(value)))
```

`<script setup>` is the compiler sugar on top: every top-level binding is automatically exposed to
the template, so there is no `return { … }` at the bottom listing everything twice. Imported
components are available in the template just by being imported.

The second, larger payoff: **logic can leave the component**. An Options API component could only
share logic through mixins, which merged invisibly and collided on names. Composition API logic
extracts into a plain function — a composable (§2.11).

### 2.2 Reactivity: `ref`, `reactive`, and `.value`

`ref()` wraps any value in a reactive box. `reactive()` makes an object reactive directly.

```js
const loading = ref(false)              // composables/useAsync.js
const values = reactive({ ...initial }) // composables/useForm.js
```

**Why `.value` exists.** JavaScript cannot detect reassignment of a variable. If `ref` returned the
raw number, `count = 5` would be an ordinary assignment and Vue would never know. Wrapping the value
in an object means the assignment becomes `count.value = 5` — a property *set*, which a proxy can
intercept. `.value` is not ceremony; it is the only place Vue can hook into.

In templates Vue unwraps top-level refs for you, which is why `loading` works in markup but
`loading.value` is needed in script.

**Where reactivity breaks — and it does, in this codebase.** Destructuring a `reactive` object or a
store copies values out of the proxy, and the copies are inert:

```js
const { data } = useDashboardStore()   // ✗ a plain snapshot, never updates
const { data } = storeToRefs(store)    // ✓ refs, still connected
```

That is why `storeToRefs` appears in five components. `ref` is the default here for exactly this
reason — it survives being passed around, because the box travels with the value. `reactive` is used
in only three places, all of them form value bags where the object is never destructured.

**One subtlety worth knowing**, because it bit me twice while building this: a plain object holding
refs — like what `usePagination()` returns — is *not* unwrapped in templates. Hence:

```vue
:page="pagination.page.value"   <!-- .value is required here -->
```

Vue only auto-unwraps refs that are top-level setup bindings, not refs nested inside a plain object.

### 2.3 `computed` — derived state

A computed is a value defined by other values. It caches, and only recalculates when a dependency
actually changes.

```js
// features/creditCards/components/CreditCardTile.vue
const utilisation = computed(() => {
  if (!props.card.creditLimit) return 0
  return (props.card.usedCredit / props.card.creditLimit) * 100
})
```

**Why not a method?** `utilisation()` would recompute on *every* render, including renders triggered
by something unrelated. More importantly, a computed declares intent: this is a value derived from
state, not an action. The same rule as the backend's "store facts, compute conclusions" — the
frontend never stores what it can derive.

### 2.4 `watch` vs `watchEffect` — and why watchers are usually wrong

A watcher runs a side effect when something changes. It is the right tool for exactly that:
*something changed, now go do something that isn't rendering*.

```js
// features/purchases/views/PurchasesView.vue
watch(pagination.page, load)   // page changed → fetch that page
```

```js
// components/layout/AppShell.vue
watch(() => route.path, () => { drawerOpen.value = false })
watch(collapsed, (value) => localStorage.setItem(STORAGE_KEY, String(value)))
```

Navigation, persistence, fetching. All genuine side effects.

**Why watchers are usually the wrong tool.** The common mistake is using a watcher to keep one piece
of state in sync with another:

```js
// ✗ don't
watch(() => props.card, (card) => { utilisation.value = card.used / card.limit * 100 })

// ✓ do
const utilisation = computed(() => props.card.used / props.card.limit * 100)
```

The watcher version introduces a second source of truth that can drift, runs a tick later, and has
to handle initialisation. The computed cannot drift — it *is* the derivation. **Reach for `computed`
first; use `watch` only when the reaction is not a value.**

`watchEffect` tracks its dependencies automatically instead of listing them. It is not used anywhere
here — with implicit tracking it is easy to accidentally depend on something and get surprise
re-runs, and every watcher in this app has one obvious trigger worth naming explicitly.

### 2.5 Lifecycle hooks

```js
onMounted(load)                         // most views: fetch once the component exists
onMounted(() => {                       // components/layout/AppHeader.vue
  document.addEventListener('click', onDocumentClick)
  document.addEventListener('keydown', onEscape)
})
onUnmounted(() => {                     // …and always the matching removal
  document.removeEventListener('click', onDocumentClick)
  document.removeEventListener('keydown', onEscape)
})
```

`onMounted` means the DOM exists and it is safe to touch it or start a request. `onUnmounted` is
where anything global gets undone. **Every global listener needs its removal** — the user menu's
outside-click handler lives on `document`, and without cleanup each navigation would leave another
one behind, all firing forever. That is a memory leak with a visible symptom.

`BaseModal` also uses `onUnmounted` to restore `document.body.style.overflow`, so a modal destroyed
while open cannot leave the page permanently unscrollable.

### 2.6 Props — one-way data flow

Props are declared with types and defaults, never as a bare array:

```js
// components/base/BaseButton.vue
defineProps({
  variant: {
    type: String,
    default: 'primary',
    validator: (v) => ['primary', 'secondary', 'ghost', 'danger'].includes(v)
  },
  loading: { type: Boolean, default: false }
})
```

The `validator` catches a typo'd variant in dev with a clear warning instead of silently rendering an
unstyled button.

**Data flows one way: down.** A child never mutates a prop. If it could, a value could change from
two directions and you would have no way to know which write won. When a child needs to change
something the parent owns, it *asks* — by emitting.

### 2.7 Emits and `v-model` on a custom component

```js
const emit = defineEmits(['update:modelValue', 'saved'])
```

Declaring emits documents the component's outward interface and lets Vue distinguish real events
from stray attributes.

**`v-model` is not magic.** On a custom component, `v-model="open"` compiles to exactly:

```vue
:model-value="open"  @update:model-value="open = $event"
```

So a component supports `v-model` by taking a `modelValue` prop and emitting `update:modelValue`.
That is the whole contract — `BaseInput`, `BaseSelect`, `BaseModal`, `BaseTabs` and `BaseProgress`
all implement it.

**Named `v-model`** lets one component carry two bound values, used by the payment source picker:

```vue
<!-- features/creditCards/components/CardPaymentModal.vue -->
<PaymentSourcePicker
  v-model:source-type="values.sourceType"
  v-model:source-account-id="values.sourceAccountId"
/>
```

which is `:source-type` + `@update:source-type`, twice.

### 2.8 Slots — and the `BaseTable` worked example

A slot is a hole in a component that the parent fills.

**Default slot** — `BaseCard`'s body. **Named slots** — `BaseCard`'s `#actions` and `#footer`,
`BaseModal`'s `#footer`, `BaseButton`'s `#icon`.

**Scoped slots** are the interesting one: the child passes data *back up* to the markup the parent
provided. `BaseTable` is the worked example.

The table knows how to lay out rows, handle the responsive fallback and render empty states. It does
**not** know that money is formatted with `Intl.NumberFormat`, or that inbound movements are green.
So it exposes one scoped slot per column:

```vue
<!-- components/base/BaseTable.vue -->
<slot :name="`cell-${column.key}`" :row="row" :value="valueOf(row, column.key)">
  {{ valueOf(row, column.key) }}
</slot>
```

and callers fill in only the cells that need it:

```vue
<!-- features/accounts/components/MovementsTable.vue -->
<template #cell-amount="{ row }">
  <span class="numeric" :class="row.isInbound ? 'is-in' : 'is-out'">
    {{ row.isInbound ? '+' : '−' }}{{ format(row.amount, { currency: row.currency }) }}
  </span>
</template>
```

The slot's *content* comes from the parent; the `row` and `value` it works with come from the child.
The fallback inside the `<slot>` tags means a column with no template still renders its raw value.

This is what stops `BaseTable` needing to know anything about finance — the requirement from the
architecture that base components stay domain-agnostic.

### 2.9 Directives

**`v-if` vs `v-show`.** `v-if` removes the element from the DOM; `v-show` only toggles
`display: none`. `v-show` is cheaper to toggle repeatedly but always pays the cost of rendering.
This codebase uses **`v-if` exclusively** — the things being toggled are modals, error states and
empty states, none of which flip fast enough for `v-show` to matter, and several of which should not
exist in the DOM at all when hidden.

**`v-for` and `:key`.** The key tells Vue which item is which between renders. Without a stable key
Vue matches by position, so inserting a row at the top makes it "reuse" the wrong DOM nodes —
component state and focus land on the wrong item. Keys here are always entity ids:

```vue
<AccountCard v-for="account in accounts" :key="account.id" :account="account" />
```

Where no id exists, the key is composed of the fields that make it unique, e.g.
`` `${item.kind}-${item.entityId}-${item.dueDate}` `` in the dashboard's upcoming list. **Never use
the array index** unless the list is static and unordered.

**Shorthands.** `:prop` is `v-bind:prop`, `@event` is `v-on:event`, `#name` is `v-slot:name`.
`v-bind="fadeUp()"` with no argument spreads a whole object of props — how the motion presets are
applied in §2.12.

### 2.10 Template refs

A template ref reaches a real DOM node:

```js
// components/base/BaseModal.vue
const panel = ref(null)              // ← matches ref="panel" in the template
const focusable = panel.value.querySelectorAll(FOCUSABLE)
```

Used only where the DOM genuinely must be touched: the modal's focus trap needs real elements to
focus, and `AppHeader` needs `menuRoot` to ask "was this click inside me?".

**`defineExpose` is not used anywhere in this app**, which is worth stating. It exists to let a
parent call a child's method through a template ref (`modalRef.value.open()`). This codebase drives
children through props and `v-model` instead — the parent sets state, the child reacts. That keeps
the data flow one-directional. `defineExpose` is the escape hatch for when a child owns imperative
behaviour a parent must trigger (focusing an input, playing a video); nothing here needed it.

Likewise **`provide`/`inject` is unused**. It passes data down an arbitrarily deep tree without
threading props through every level. The tree here is shallow, and the two things that genuinely
needed to be reachable from anywhere — auth and toasts — are stores, which solve the same problem
without the implicit coupling of an injection key.

### 2.11 Teleport, Transition, TransitionGroup

**`Teleport`** renders markup somewhere else in the document while keeping it logically in the
component. `BaseModal` and `BaseToast` teleport to `<body>`:

```vue
<Teleport to="body">
```

Without it, a modal rendered deep inside the page inherits ancestor `overflow: hidden`,
`transform` and `z-index` stacking contexts — the classic "my modal is trapped inside a scrolling
card" bug. Teleporting to `body` escapes all of that.

**`Transition`** animates a single element entering or leaving: the modal, the user menu, the page
transition in `App.vue`, the offline banner. **`TransitionGroup`** does the same for a *list* and
additionally animates reordering, which is why toasts slide in and the remaining ones move up when
one is dismissed.

### 2.12 Composables

A composable is a plain function that uses Vue's reactivity and returns reactive state. The `use`
prefix is convention.

```js
// composables/useAsync.js
export function useAsync(fn, { immediate = false, initialData = null } = {}) {
  const data = ref(initialData)
  const error = ref(null)
  const loading = ref(false)

  async function run(...args) { … }

  return { data, error, loading, run, reset }
}
```

**Composables vs mixins.** Mixins merged an object into a component: you could not tell where a
property came from, two mixins could silently collide on the same name, and there was no way to use
one twice. A composable is just a function call — you can see what it returns, rename it on
destructure, and call it multiple times. Every problem with mixins comes from implicitness;
composables are explicit.

**Composables vs stores.** This is the distinction that decides where new code goes:

> A **composable** creates *fresh state per caller*. A **store** is a *single shared instance*.

`useAsync()` in two components gives two independent `loading` flags — correct, because those two
components load different things. `useAuthStore()` in two components gives *the same* user — also
correct, because there is one logged-in user.

That is why `useMoney` is a composable even though it reads from a store: it returns formatting
*functions*, and every caller wants its own. Auth is a store because the token is one thing that
several unrelated parts of the app must agree on.

**The composables**: `useAsync` (one request's three states), `useForm` (values + the API's two
error shapes), `usePagination` (mirrors the paging envelope), `useMoney` (all currency formatting,
plus `roundCents`), `useToast` (ergonomics over the UI store), `useMediaQuery` (breakpoints that
change *behaviour*, not just appearance), `useMotionSafe` (reduced-motion presets), `useOnlineStatus`
(offline banner), `useDateTime` (timestamps, §8.13), `useDoubleConfirm` (two-step destructive
confirm), and three for language: `useI18n`, `useServerText`, `useAlertText` (§9).

`useServerText` and `useAlertText` also export plain functions, not just composables. The axios
interceptor has to translate error messages and has no component context, so `serverError` is
callable from anywhere.

---

## 3. Pinia

### 3.1 What a store is, and when you need one

A store is state that lives outside the component tree, so unrelated components can read and write
the same thing.

Most state does **not** need this. Props and emits handle a parent talking to its children, and that
covers the overwhelming majority of cases. A store earns its place only when state is shared across
components that have no parent-child relationship.

**The rule used here: create a store only when two unrelated places must agree on one value.**

The four stores, and the specific problem each solves:

| Store | Why it must be shared |
|---|---|
| `auth.store.js` | The router guard, the header and every API call need the same token |
| `ui.store.js` | Any component raises a toast or confirm; one component near the root renders them |
| `notifications.store.js` | The header badge and the notifications screen must never disagree |
| `dashboard.store.js` | Aggregates are read by the dashboard and invalidated by twelve other screens |

Everything else — every list, every form, every detail view — uses `useAsync` locally. A store per
feature would have meant fourteen stores of state that exactly one component reads.

The notifications store is the clearest justification: mark a notification read on the list and the
header badge decrements immediately, because there is one `unreadCount`, not two.

### 3.2 The Composition-API store syntax

Pinia offers two syntaxes. This codebase uses the setup style throughout, because it is the same
mental model as a component:

```js
export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('wm_token'))       // state
  const isAuthenticated = computed(() => Boolean(token.value)) // getter
  async function login(credentials) { … }                    // action

  return { token, isAuthenticated, login }
})
```

`ref` is state, `computed` is a getter, a function is an action. Nothing new to learn. The
alternative (options style, with `state`/`getters`/`actions` keys) is closer to the old Options API
and separates by kind rather than concern — the same trade-off as §2.1.

### 3.3 Why actions return booleans instead of throwing

```js
async function login(credentials) {
  loading.value = true
  error.value = null

  try {
    persist(await authApi.login(credentials))
    return true
  } catch (err) {
    error.value = err
    return false
  } finally {
    loading.value = false
  }
}
```

The caller reads:

```js
const ok = await auth.login({ ...values })
if (!ok) { /* show auth.error */ return }
router.replace(redirect)
```

**The alternative** — letting the action throw — forces every call site into a `try/catch` and makes
it easy to forget one, at which point an unhandled rejection surfaces in the console instead of the
UI. Returning a boolean and parking the error on the store makes the failure path impossible to skip
accidentally: the `if` is right there.

`useAsync` follows the same convention: `run()` resolves to the data or `null`, and never throws.

### 3.4 Persistence

Auth persists by hand, on write:

```js
localStorage.setItem('wm_token', token.value)
localStorage.setItem('wm_user', JSON.stringify(user.value))
```

and reads back in the initialiser, so a refresh keeps you signed in. `AppShell` persists the sidebar
collapse the same way, through a `watch`.

There is no persistence plugin. Two keys and one boolean did not justify a dependency, and doing it
explicitly makes it obvious *what* is stored — which matters, since one of those keys is a
credential (§6.3).

---

## 4. Vue Router

### 4.1 Route config and lazy loading

Every route's component is a dynamic `import()`:

```js
{
  path: '/accounts',
  name: 'accounts',
  component: () => import('@/features/accounts/views/AccountsView.vue')
}
```

**Why every route.** A static import puts the component in the main bundle, so a user who only opens
the dashboard still downloads the reports screen, the job forms and everything else. A dynamic import
makes Vite emit a separate chunk fetched on first visit. The build output shows this working:
`JobView` (21 kB), `GoalsView` (15 kB) and the rest are separate files, and `motion` sits in its own
122 kB chunk that the login screen never touches.

The cost is a brief fetch on first navigation to each route — invisible on a local network, and worth
it for a first paint that is not carrying nineteen screens.

### 4.2 Layouts

There are no nested routes here. Layout is chosen by route metadata instead:

```js
meta: { public: true, layout: 'blank' }   // auth screens
```

```vue
<!-- App.vue -->
const useShell = computed(() => route.meta.layout !== 'blank')
```

**Chrome is the default and `blank` is the opt-out**, which is the safer direction: a new route that
forgets the flag gets full navigation, rather than a page stranded with no way out.

Nested routes (a parent route rendering `<RouterView>` inside a layout component) are the other
common approach and are better when layouts nest more than one level deep. With exactly two layouts,
the metadata flag is less machinery for the same result.

### 4.3 Params vs query

**Params** are part of the resource's identity: `/accounts/:id`. Read with `route.params.id`.

**Query** is a modifier on the same resource: `?redirect=`, `?page=`, `?targetType=`. Read with
`route.query`. Filters belong in the query because a filtered list is still the same list — and it
means the URL can be shared or bookmarked.

### 4.4 The guard, and protected-by-default

```js
router.beforeEach((to) => {
  const auth = useAuthStore()

  if (!to.meta.public && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.public && auth.isAuthenticated && to.name !== 'not-found') {
    return { name: 'dashboard' }
  }

  return true
})
```

**Why opt-out beats opt-in.** If routes were public by default and each protected one had to be
marked, then forgetting the flag leaks a screen — a silent failure that testing while logged in will
never reveal. With protected-by-default, forgetting the flag on a *public* route bounces you to
login, which is immediately obvious and harmless. **The failure mode of a mistake should be
inconvenient, not dangerous.**

This is the same reasoning as the backend's user-scoped-by-default repositories.

The guard also runs the reverse check, so a signed-in user hitting `/login` is redirected to the
dashboard instead of seeing a form they do not need.

### 4.5 The `?redirect=` flow

1. A signed-out user opens `/accounts/abc-123`.
2. The guard redirects to `/login?redirect=/accounts/abc-123`.
3. `LoginView` reads it after a successful sign-in:

```js
const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/'
router.replace(redirect)
```

They land where they were going, not on a generic dashboard. The `typeof` check matters: query values
can be arrays (`?redirect=a&redirect=b`), and passing an array to `replace()` would throw.

`replace` rather than `push` so the login page does not sit in history — pressing Back after signing
in should not return to a form you already completed.

**Note for a production hardening pass:** `redirect` is used unvalidated. Because it is passed to
`router.replace()` it can only resolve to an in-app route, so it cannot become an off-site open
redirect — but a stricter version would assert it starts with `/`.

---

## 5. The architecture

```
src/
├── api/           URLs and payload shapes. No state.
├── i18n/          en.js, es.js and the lookup. No Vue in it.
├── stores/        Shared state. Calls the api layer.
├── composables/   Reusable reactive logic. No markup.
├── components/
│   ├── base/      Domain-agnostic. Knows nothing about money.
│   └── layout/    App chrome: shell, sidebar, header, page header.
├── features/      One folder per domain area: views/ + components/
└── assets/styles/ Every stylesheet (§7.6). Components hold no CSS.
```

**The dependency direction.**

```
features ──► stores ──► api
    │           │
    └──► composables ──► api
    │
    └──► components/base
```

Rules, and what each one prevents:

- **`api/` never holds state.** It is a thin map of URL to function. If it cached, two callers would
  disagree about when data is fresh.
- **`components/base/*` never imports from `features/` or `api/`.** `BaseTable` renders any data;
  the moment it knows about `useMoney` it is no longer reusable, and the scoped-slot design (§2.8)
  exists precisely so it never needs to.
- **Components never call axios directly.** They call a store or an api module, so a change to how
  requests are made has one place to change.
- **Feature components may know the domain.** `AccountCard` knows about blocked savings accounts;
  that is its job.

### 5.1 One action, end to end

Tracing "the user pays a credit card" through every layer:

**1 — Click.** `CreditCardTile` does not know how to pay. It emits:

```vue
<BaseButton :disabled="card.usedCredit <= 0" @click="$emit('pay', card)">
```

The `disabled` is already domain knowledge: the API rejects payments on a card that owes nothing, so
the button is not offered.

**2 — View.** `CreditCardsView` catches the event, sets which card is active, opens the modal.

**3 — Modal opens.** A `watch` on `modelValue` resets the form and fetches accounts — deliberately on
open, not on mount, so the list is fresh each time.

**4 — Form.** `useForm` holds the values. `PaymentSourcePicker` filters accounts to the card's
currency and disables blocked ones — three API rules encoded as *unavailable options* rather than
future error messages.

**5 — Submit.** `useForm.submit()` calls the submit function, which shapes the payload:

```js
sourceAccountId: payload.sourceType === PAYMENT_SOURCE.ACCOUNT ? payload.sourceAccountId : null
```

because the API rejects a payload naming both.

**6 — API layer.** `creditCardsApi.pay(id, body)` → `client.post()`.

**7 — Interceptors.** The request interceptor attaches the JWT. The response interceptor unwraps
`response.data`, so the caller gets the DTO, not an axios envelope.

**8 — Result.** On success `useForm` returns the DTO; the modal raises a toast, emits `saved` and
closes. On failure the interceptor has already normalised the error, and `useForm` splits it: field
errors to the inputs, anything else to the banner.

**9 — Invalidation.** The view reloads the cards *and* calls `dashboard.invalidate()`, because paying
a card changed the totals on a screen the user is not currently looking at.

**10 — Re-render.** `cards` is a `ref` inside `useAsync`; assigning to it re-renders the grid. The
tile's `utilisation` computed recalculates because its dependency changed. Nothing is manually
refreshed.

Nine layers, and no component ever knew about axios, no base component ever knew about money.

---

## 6. The API integration

### 6.1 The client and its two interceptors

```js
const client = axios.create({ baseURL: '/api/v1' })
```

**Request interceptor** — attaches the token to every call:

```js
config.headers.Authorization = `Bearer ${token}`
```

Doing it here rather than per call means it is impossible to forget, and there is one place to change
if the scheme ever does.

**Response interceptor** — does three jobs:

1. **Unwraps** `response.data`, so callers get the payload directly.
2. **Handles 401** by clearing credentials and redirecting to login — an expired token logs you out
   from wherever you are.
3. **Normalises errors** into one shape.

### 6.2 Why two backend error shapes become one

The API answers failures in two formats: validation errors keyed by field, and business-rule
violations with a single message.

```json
{ "title": "Validation failed", "errors": { "Amount": ["Must be greater than zero."] } }
{ "title": "Business rule violation", "detail": "Insufficient funds in 'Checking'." }
```

Every consumer becomes:

```js
{ status, message, fields }   // fields is null when it is not a field error
```

**Why normalise.** Without it, every form would test both shapes and every non-form caller would
handle a case it does not care about. With it, there is one rule: **forms read `error.fields`,
everything else reads `error.message`** — which is exactly the split `useForm` makes.

The normaliser also lowercases the first letter of each key, turning the API's PascalCase `Amount`
into `amount` so it lines up with a form field of the same name without any mapping table.

### 6.3 JWT storage, and its honest downside

The token is in `localStorage`.

**The trade-off, stated plainly: this is vulnerable to XSS.** Any injected script on the page can
read `localStorage` and exfiltrate the token. The safer alternative is an `httpOnly` cookie, which
JavaScript cannot read at all — that removes token theft via XSS entirely.

Why it was not used here: `httpOnly` cookies require the backend to set them, CORS configured with
credentials, and CSRF protection (a cookie is sent automatically, so a forged cross-site request
carries it — the exact problem a bearer header does not have). That is a backend change, and the
backend is fixed for this project.

**Realistically:** for a single-user personal finance app on localhost this is fine. For a public
deployment it is the first thing to revisit, and it is a backend-and-frontend change, not a frontend
one.

Note the token is *not* refreshed. When it expires the next request returns 401 and the interceptor
signs you out. Silent refresh would need a refresh-token endpoint, which does not exist.

### 6.4 The dev proxy, and CORS in production

```js
server: { proxy: { '/api': { target: 'http://localhost:5015' } } }
```

The app runs on port 5173 and the API on 5015. Different ports are different **origins**, so the
browser would block the request under the same-origin policy. The dev proxy sidesteps this: the app
requests `/api/...` on its *own* origin, and Vite forwards it server-side, where same-origin policy
does not apply. No CORS involved at all.

**This does not exist in production.** `vite build` emits static files; there is no dev server to
proxy. Two options:

1. **Serve both from one origin** — put the built `dist/` behind the same host as the API (reverse
   proxy, or the API serving the static files). Still no CORS. Simplest, and it keeps `baseURL`
   unchanged.
2. **Separate hosts** — then the API must send CORS headers (`Access-Control-Allow-Origin` for the
   app's origin, and `Allow-Headers: Authorization`), and `baseURL` must become the absolute API URL,
   probably from an env var.

The backend currently configures no CORS policy, so **option 1 works as-is and option 2 needs a
backend change**.

---

## 7. The design system

### 7.1 The tokens

All defined in `assets/styles/_tokens.scss` as CSS custom properties on `:root`.

| Token | Role |
|---|---|
| `--canvas` `#F3F2EE` | Page background |
| `--canvas-alt` `#ECE9E2` | Sidebar, table stripes, tinted panels |
| `--surface` `#FFFFFF` | Cards |
| `--line` `#E9E9E7` | Subtle dividers |
| `--ink` `#201F1D` | Text |
| `--text-muted` / `--text-subtle` | Secondary / tertiary text |
| `--accent` `#212F46` | Primary actions |
| `--gold` `#CBB697` | Highlights (the top expense) |
| `--positive` / `--negative` / `--warning` | Money semantics |
| `--border-color` `#5C5A55` | Outlines |
| `--shadow-color` `#BDBAB2` | The flat shadow |

Plus `--sp-1…16` spacing, `--fs-xs…2xl` type, `--fw-*` weights, `--radius`, `--ease`, `--dur`.

### 7.2 Why CSS custom properties, not SCSS variables

An SCSS variable is compiled away — `$accent` becomes a literal hex in the output, and nothing can
change it afterwards. A CSS custom property is a **live value in the browser**.

Three practical consequences:

1. **One line changes everything.** Softening the shadow and border was two token edits that
   propagated to every card, button and table in the app. With SCSS variables it would have been the
   same edit plus a rebuild — but only because the values happened to be centralised; anything that
   had inlined a literal would have drifted.
2. **Runtime theming is possible without a rebuild.** A dark theme is a second `:root` block under a
   media query or a `[data-theme]` attribute. With SCSS variables it would require compiling two
   stylesheets.
3. **They cross the scoped-style boundary.** A `<style scoped>` block cannot see a `$variable` from
   elsewhere unless the file is imported into it, but `var(--accent)` works everywhere because it is
   resolved by the browser, not the compiler.

SCSS is still used for what it is genuinely good at: nesting, and **mixins** (`flat-card`,
`pressable`, `focus-ring`, `truncate`), which emit multiple declarations — something a custom
property cannot do. Mixins are injected into every component automatically via
`css.preprocessorOptions.scss.additionalData` in `vite.config.js`, so no component carries an
`@use` line.

### 7.3 The flat-shadow language

The signature: hard offset, **zero blur**.

```scss
--shadow: 4px 4px 0 var(--shadow-color);
```

No blur radius. The shadow is a solid displaced copy, not a soft glow — deliberately at odds with the
default "material" look, and paired with 1px borders, a 6px radius and a warm off-white canvas.

`pressable` completes it: on `:active` the element translates 2px and its shadow shrinks, so it
appears to sink into its own shadow.

The same language is reproduced in the **PDF report**, where QuestPDF has no `box-shadow` — it is
faked with nested backgrounds so the offset shows along two edges (see
`MonthlyReportPdfGenerator.cs`).

### 7.4 Responsive strategy

Three breakpoints, each with a specific job:

| Width | What changes |
|---|---|
| `≤ 640px` | Card grids collapse to one column; forms to a single column |
| `≤ 767px` | **Tables become stacked cards**; headers wrap; padding tightens |
| `≤ 1023px` | Sidebar becomes an overlay drawer |
| `≥ 1024px` | Sidebar is a column, collapsible to a 64px icon rail |

The table-to-cards switch at 767px is the one that matters most: a five-column table is unreadable at
360px, and horizontal scrolling is a poor answer. `BaseTable` renders **both** a `<table>` and a
stacked-card list from the same column config and the same scoped slots, showing one and hiding the
other. The cost is duplicated DOM; the benefit is that a caller writes its cell templates once and
gets both layouts.

CSS handles nearly all of it. `useMediaQuery` exists only for the case where a breakpoint changes
*behaviour* rather than appearance — the sidebar toggle means "collapse" on desktop and "open drawer"
on mobile, which is a JavaScript decision.

### 7.5 Base component API conventions

Consistency across the 18 base components:

- **`v-model`** for the primary value (`modelValue` + `update:modelValue`).
- **`variant`** for appearance, **`size`** for scale, both validated.
- **`error`** accepts a string or array — the API sends arrays, hand-written errors are strings.
- **Named slots** for optional regions: `#icon`, `#actions`, `#footer`, `#prefix`, `#suffix`.
- **Scoped slots** for per-item rendering: `#cell-{key}`.
- **Emits declared explicitly**, always.
- **Never any domain knowledge.**

### 7.6 Where the styles live

No component holds CSS. Each has a stylesheet under `assets/styles`, grouped the way the components
are:

```
assets/styles/
├── _tokens.scss  _mixins.scss  main.scss   globals, injected or imported once
├── base/          App.scss — the only deliberately unscoped sheet
├── components/    the 18 Base* primitives
├── layout/        shell, header, sidebar, page header
└── features/      one folder per feature; components and views together
```

A component points at its sheet and keeps the tag:

```html
<style scoped lang="scss" src="@/assets/styles/features/accounts/AccountCard.scss"></style>
```

**The tag stays because it is what carries `scoped`.** Deleting it and importing the SCSS from
`<script setup>` compiles perfectly and makes every rule global — styles leaking between components,
which is the kind of breakage you notice somewhere else entirely, much later.

Vite injects the mixins into every SCSS block through `additionalData`, and that still applies
through `src=`, so no stylesheet needs an `@use` line. Component and view styles share one folder per
feature rather than nesting further: every basename in the app is unique, so the extra level would
add depth without saying anything.

The trade is that a stylesheet is now a folder away from its component, so renaming a component does
not drag its styles along.

---

## 8. The feature modules

### 8.1 Auth — `/login`, `/register`
Blank layout via `meta.layout`. Field errors from `error.fields`, form-level errors in a banner.
Honours `?redirect=`. Registration captures the chosen currency into the auth store, because the API
does not return it (§9).

### 8.2 Dashboard — `/`
The payoff screen. Four stat tiles, alerts, upcoming dues, month summary, goals. Loads dashboard and
alerts in one `Promise.all`. Three subtleties are surfaced rather than hidden: `installmentRemaining`
is shown as context and never added to a total (it is already inside `totalUsedCredit`),
`monthlyObligations` is labelled "Committed — loans + next installments" so the exclusion of
revolving card debt is visible, and `excludedCurrencies` gets its own notice explaining why other
currencies are not converted. The alerts panel is collapsible; the count stays visible when closed.

### 8.3 Accounts — `/accounts`, `/accounts/:id`
Card grid with per-currency totals (never summed across currencies). Deposit, withdraw, transfer,
block/unblock. The transfer modal excludes the source account and any different-currency account
from the destination list. Detail view shows paged movements with the stored `balanceAfter` — the
running balance is a recorded fact from the API, never recomputed in the browser.

### 8.4 Credit cards — `/credit-cards`, `/credit-cards/:id`
Utilisation bars (amber at 50%, red at 80%). Limit updates have their own endpoint and their own
modal. Payments use the shared `PaymentSourcePicker`; "Pay" is disabled at zero balance.

Tiles show the billing cycle as **dates**, not day numbers, and split what is owed rather than
showing one total. "Owed $100" is not actionable; "$50 due Aug 15, $50 not billed until Sep" is.
The tile carries `statementBalance` and `currentCycleCharges` with their deadlines beside them, plus
a line for `futureInstallments` when a plan is running — shown only then, because otherwise the
figures would not visibly add up to the total and the reader would hunt for the gap. The detail view
shows all three as a grid under the progress bar.

Every figure is computed server-side (`StatementCycle.Split`) and arrives on the DTO. The client
does no arithmetic on money here — same rule as `balanceAfter` in §8.3.

The payment modal names the statement balance beside the total owed, and its success toast reports
any installments the payment settled. Both exist for the same reason: the balance changing is
visible and the schedule moving is not, so a user who is not told would go and pay the same
installment a second time.

**Three tabs:** charges, installments, payments. The charges tab lists a plan on the day it was
created, at its full price — which is what put the debt on the card. The installments tab answers the
different question of what those plans cost *this month*: each product, its progress, what is left,
and what it adds to the statement about to be billed, with the total in the footer. A plan showing
`0.00` there has already had its installment paid; it stays visible and muted rather than
disappearing, so the column keeps lining up.

The due date comes from the server's `nextDueDate` — the first due day *after* the next cutoff — so
the card screen and the dashboard's safe-to-spend figure cannot disagree about the same money.
Tracking fields on this form are covered in §8.14.

### 8.5 Payments — `/payments`
User-wide ledger across cards, debts and installments, with date and target-type filters. Source is
labelled "Cash / third party" rather than the raw `External`.

### 8.6 Purchases & stores — `/purchases`, `/stores`
The purchase form swaps its required field by payment method and clears the previous instrument on
switch, matching the API's constructor invariant. Cash is the only method that asks for a currency.
Stores are a shared catalogue: `isMine` controls whether an edit button renders, and a broken
`logoUrl` falls back to a monogram. The store picker creates inline and selects what it created.

### 8.7 Installments — `/installments`, `/installments/:id`

The detail screen names the **card that was charged**, linked, along with what the plan adds to that
card's next statement and when it is due. A plan is meaningless without its card — the card decides
when the installments fall due — and a screen that cannot say which balance this is part of leaves
the user to go and find out.

Progress is labelled **payments left**, not months left, and the plan shows the date of its *last
payment* rather than an implied duration. The two are the same number until someone pays ahead:
settling two installments in one month leaves 10 payments outstanding, but they are still
installments 3–12, so the final one falls on its original date. Calling the count "months" invited
the reading "10 months from now", which stops being true the moment a plan is prepaid.
`previewSchedule()` reproduces the backend's split so the real instalment amounts are shown before
submitting, including the last-payment remainder. Warns when the total exceeds available credit,
because a tasa 0 plan charges the card in full on day one. Paying takes no amount — the endpoint pays
the oldest unpaid row, so the modal shows which one that is.

### 8.8 Debts — `/debts`, `/debts/:id`
Status drives the affordances: "Default" only on Active debts, "Pay" disabled once paid off, and the
payment modal notes that paying a defaulted debt reactivates it. Amount pre-fills with the scheduled
monthly payment.

### 8.9 Goals — `/goals`
Savings and product goals share one card and one form, switched by a `kind` prop. Linked savings goals
show a source picker and perform a real transfer; unlinked and product goals show none and say why.
`requiredMonthlyContribution` renders only when the API sends one — absent for a goal with no
deadline, which is an honest gap rather than a zero.

### 8.10 Job & income — `/job`
Live net-salary preview via `computeNet()`, which mirrors the domain rule, and turns red when
deductions would exceed gross. Payment days are three selects, deduplicated before sending. Deduction
mutations return the whole job, so state is replaced without a refetch.

### 8.11 Reports — `/reports`
Native `<input type="month">` produces the `yyyy-MM` the route wants. Full on-screen breakdown plus a
PDF download via blob → object URL → click → revoke. The cash-withdrawn note explains why it is
excluded from the net result.

### 8.12 Notifications — `/notifications`
Unread filter, mark-as-read, and "Check now" which distinguishes created / nothing-new / failed.
Explains that reading is an acknowledgement, not a mute.

### 8.13 Timestamps across the app

History tables show date over time in a stacked cell (`BaseTimestamp`), so the hour fits without
widening any date column and the date stays the thing you scan down the page. Everything the API
sends is UTC; `useDateTime` renders in the viewer's own zone, because "when did I spend this?" means
local time.

One trap worth keeping: **a date-only string is a calendar date, not an instant.**
`new Date('2026-08-13')` is UTC midnight, which displays as the 12th anywhere west of Greenwich — a
payment due the 13th reading as due the 12th. `toDate` detects `YYYY-MM-DD` and builds it from local
parts; real timestamps still convert normally.

The purchase form captures date *and* time. It used to take a date alone, which would have made
every purchase read `00:00` and the hour column dead weight. The `datetime-local` value is local
wall-clock with no zone, so it is converted explicitly — `toISOString()` on it would shift what the
user typed by their offset.

### 8.14 Instrument tracking fields

Both the account and card forms carry two extra fields, rendered by one shared
`features/shared/components/TrackingFields.vue`:

- **Last 4 digits** — optional, `maxlength=4`, non-digits stripped as you type, mirroring the
  server's `^\d{4}$`.
- **Tracking mode** — a radio pair. **Manual** is the default and the only selectable option;
  **Automatic** is rendered **disabled** behind a "Coming soon" `BaseBadge`.

The automatic value round-trips and persists correctly — it simply cannot be chosen, because the
ingestion that would honour it is not built (see "Planned: automatic transaction sync", §6.14 of the
project guide). An option that silently does nothing would be worse than one that says it is not
ready.

One component rather than the same markup in two forms: the pair is governed by a single rule (sync
requires digits), and two copies would drift the moment that rule or the disabled state changes.

**Editing sends the tracking pair only when it changed.** It has its own endpoint, separate from the
ordinary update, so an unchanged pair would otherwise be a second request that writes nothing.

Where accounts and cards are listed, a present `lastFour` renders as `••••7765` in muted `.numeric`
text beside the bank name. Absent, nothing is shown — a placeholder would imply data that is simply
not there.

`BaseInput` gained a `maxlength` prop for this. The component's root is the wrapping `div`, so a
bare `maxlength` attribute would have fallen through onto the div and done nothing.

### 8.15 Settings — `/settings`

Bank defaults: which account to assume when a bank's transfer notification names none. A table, an
upsert form (bank name, direction, account), and delete behind the double confirmation used
everywhere destructive.

The form always sends the same request whether creating or editing, because the endpoint upserts on
`(bankName, direction)` — there is no id to send and no separate create path.

The account dropdown lists only non-archived accounts, matching the server, which 404s an archived
one: a fallback that could never be honoured is refused at the point of nomination.

Its empty state is **informational, not an error**. Having no defaults is the normal starting
position and nothing is broken without them, so the copy says so rather than prompting for a fix.

---

## 9. Language

Two languages, English and Spanish, chosen from a selector in the user menu.

### 9.1 The mechanism

Hand-rolled rather than `vue-i18n`: two locales with `{placeholder}` interpolation is about seventy
lines, and the `t(key, params)` call shape is deliberately vue-i18n's, so swapping later would not
touch a call site.

- `i18n/en.js`, `i18n/es.js` — the copy. English is the reference; a dev-only check reports keys
  present in one and not the other, and mismatched placeholders.
- `stores/locale.store.js` — the chosen language. A **store, not a module-level ref**, so every
  component re-renders on a change; switching must not need a reload.
- `useI18n()` — `t`, `tc`, `locale`, `setLocale`.

The choice lives outside `auth.store` on purpose: it belongs to the browser, not the account. It
survives signing out and applies on the login screen. It persists to `localStorage`, falls back to
the browser language before English, and sets `document.documentElement.lang`.

`tc(key, count)` picks `key.one` or `key.other`. Deliberately two forms and not CLDR plural rules —
English and Spanish need exactly that, and a language with a "few" form would want this replaced
rather than extended.

### 9.2 Three ways to freeze the language by accident

All three compile, run, and look correct until someone switches language.

**A literal in a module evaluated once.** `navigation.js` is imported at load, so a label written
there freezes in whichever language was active then. Nav items carry `labelKey`, not text.

**A `const` calling `t()` in setup.** `const COLUMNS = [{ label: t('common.date') }]` runs once.
Table columns, tab strips and option lists are `computed`.

**A default resolved in the wrong layer.** `ui.store` used to default a confirm dialog's buttons to
English strings; a default resolved there is fixed at whatever language the *caller* ran in. The
store stores `null` and `BaseConfirmDialog` resolves the fallback, where the locale is reactive.

### 9.3 Copy that wraps a figure

A sentence built around an amount is **one key with a placeholder**, not fragments concatenated
around a number:

```js
checkingAndSavings: '{checking} checking · {savings} savings'
// es: '{checking} en corriente · {savings} en ahorro'
toGo: '{amount} to go'
// es: 'faltan {amount}'   ← verb first; fragments would lock English word order
```

Two keys are used in exactly one situation — where a bolded figure sits inside the sentence and
splitting is the only way to keep the emphasis. Both languages happen to place the amount
identically there, and the locale file says so.

### 9.4 What the API sends in English

The server answers in English. Two different problems, solved differently.

**`serverLabel(group, value)`** — enum names and catalogue categories are closed sets, looked up by
the exact string the server returns: movement types, payment methods and sources, account, goal and
debt statuses, income frequencies, severities, deduction types, purchase and store categories. An
unmapped value **falls through to the server's own wording**, so a member added to the API before it
is added here still reads as something.

Category dropdowns translate only the label; the value stays the English name the API stores and
filters on, so changing language cannot change what gets submitted. `IncomeFormModal` learned this
the hard way — it matched the server's frequency against the option *label*, which translating would
have silently broken into "Monthly" every time. Options carry a stable `name` for lookups now.

**`serverError(message)`** — error prose is not a closed set; the server composes it with names and
amounts inside. It is matched by shape and re-rendered from the captured parts, so
`"Insufficient funds in 'Main'. Available: 0.00 USD, requested: 128.40 USD."` keeps both figures in
Spanish. Anything unrecognised falls through to the server's English — worse than a translation, far
better than hiding what failed.

Errors are translated **in the axios interceptor**, not at each of the twenty
`toast.error(err.message)` sites, so none of them has to know and a new one cannot forget.
Field-level validation messages are deliberately left as sent.

**`useAlertText().render(alert)`** — alerts and notifications ship `params` alongside their English
text ([why](PROJECT_GUIDE.md#613-alerts-store-the-parts-not-the-sentence)). The sentence is rebuilt
from the translation for the alert's `type`, with money
and dates formatted in the viewer's locale. No params, or a type this build does not know, falls back
to the server's text.

---

## 10. Known limitations

Stated plainly, because knowing the edges is part of knowing the system.

- **The token lives in `localStorage`, which is XSS-readable.** See §6.3 for the full trade-off and
  what changing it would require.
- **Two backend calculations are duplicated in the browser.** `previewSchedule()` (installments) and
  `computeNet()` (salary) reimplement domain arithmetic so a figure can be shown *before* the request
  exists. If the backend rule changes, these go stale silently. Everything else displays only what
  the API computed.
- **The profile currency is guessed until the dashboard loads.** The auth response omits it, so a
  user who signs in (rather than registers) sees USD formatting until the dashboard corrects it.
  Recorded as [BACKEND_REQUESTS.md](BACKEND_REQUESTS.md) #1.
- **PDF download failures cannot show a real reason.** The response is a Blob, so the error
  normaliser has no JSON to read and the toast is generic.
- **The PDF's typography does not match the app.** The web client loads Inter from Google Fonts;
  QuestPDF falls back to Lato unless Inter is installed on the server or dropped into the API's
  `Fonts/` folder. Colours, layout and shapes do match.
- **PWA icons are SVG, not PNG.** Chrome installs fine; iOS Safari and some Android launchers prefer
  PNG rasters and may substitute a generic icon.
- **No profile or settings screen**, because no endpoint exists to read or update the user
  (BACKEND_REQUESTS #2).
- **Accounts and cards delete; purchases and plans do not.** The first two archive behind a
  double confirmation (BACKEND_REQUESTS #3 is resolved for them). Purchases and installment plans
  still have no endpoint, and no affordance is rendered for them.
- **Nothing restores an archived account or card.** The data is intact and the backend has
  `Restore()`, but no screen calls it.
- **Alert and notification bodies raised before the backend sent `params` stay English.** Those rows
  fall back to their stored text; only re-raised alerts follow the selector (§9.4).
- **Field-level validation messages stay English**, deliberately — they name rules and lengths that
  pattern matching would mangle rather than translate.
- **The monthly PDF prints times in UTC** while the app shows local time, so the same purchase can
  read at a different hour in each. The PDF labels its column `Date (UTC)`; the reasoning is in the
  backend guide.
- **No automated tests.** The architecture is shaped for them — composables are pure functions of
  their inputs and the API layer is trivially mockable — but none are written. The i18n layer is the
  easiest place to start: `translate`, `roundCents` and `useDateTime.toDate` are pure.
- **`?redirect=` is not validated.** It cannot leave the app because it goes through
  `router.replace()`, but see §4.5.

---

## 11. Glossary

| Term | Meaning |
|---|---|
| **Composition API** | Organising a component by concern using functions, rather than by option kind. |
| **`<script setup>`** | Compiler sugar making every top-level binding available to the template. |
| **Ref** | A reactive box around a value. `.value` exists so assignment is interceptable (§2.2). |
| **Reactive** | A reactive proxy over an object. Breaks if destructured. |
| **Computed** | Cached derived state. Recalculates only when a dependency changes. |
| **Watcher** | A side effect that runs on change. The wrong tool for deriving values. |
| **Prop** | Data passed down. Never mutated by the child. |
| **Emit** | An event sent up, so the owner of state is the one that changes it. |
| **Scoped slot** | A slot the child passes data *into*, letting the parent render with the child's data (§2.8). |
| **Teleport** | Renders markup elsewhere in the DOM to escape ancestor stacking and overflow. |
| **Composable** | A function returning reactive state. **Fresh per caller** — unlike a store. |
| **Store** | Shared state outside the tree. **One instance** — unlike a composable. |
| **`storeToRefs`** | Destructures a store while keeping reactivity intact. |
| **Lazy route** | A route whose component is a dynamic import, so it becomes its own bundle. |
| **Navigation guard** | A function that runs before navigation and may redirect it. |
| **Protected by default** | Routes require auth unless marked public, so a mistake is inconvenient rather than dangerous (§4.4). |
| **Interceptor** | Axios middleware on every request or response. |
| **Error normalisation** | Collapsing the API's two error shapes into one `{ status, message, fields }` (§6.2). |
| **Design token** | A CSS custom property naming a design decision, changeable at runtime (§7.2). |
| **Flat shadow** | A hard offset with zero blur — the app's signature (§7.3). |
| **Invalidate** | Marking dashboard aggregates stale after a mutation elsewhere. |
| **Locale** | The chosen language, `en` or `es`. Lives in its own store because it belongs to the browser, not the account (§9.1). |
| **Translation key** | A dotted path into the locale files. Missing keys fall back to English, then to the key itself, so a gap is visible rather than blank. |
| **Interpolation** | `{name}` in a string, filled at render. One key with a placeholder, never fragments around a value (§9.3). |
| **Server label** | A translation of a value the API sends — an enum name or category — looked up by the exact string, falling back to it (§9.4). |
| **Alert params** | The parts an alert sentence was built from, sent beside the English text so the sentence can be rebuilt in another language. |
| **Scoped style** | CSS limited to one component by a `data-v` attribute. Carried by the `<style>` tag, which is why the tag survives the move to external files (§7.6). |

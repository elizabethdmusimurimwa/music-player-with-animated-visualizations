// Shared validation for Login and Sign Up pages.
// Each input has a data-rule attribute that picks a rule from the "rules" object below.
// A rule returns "" when the value is fine, or an error message when it is not.

const rules = {
  username(value) {
    value = value.trim();
    if (value === "") return "Choose a username.";
    if (value.length < 3) return "Username needs at least 3 characters.";
    if (value.length > 20) return "Username can be 20 characters at most.";
    if (!/^[a-zA-Z0-9_]+$/.test(value)) return "Use only letters, numbers and underscores.";
    return "";
  },

  email(value) {
    value = value.trim();
    if (value === "") return "Enter your email address.";
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(value)) return "Enter a valid email, like name@example.com.";
    return "";
  },

  // Login only needs to know the field is not empty
  loginPassword(value) {
    if (value === "") return "Enter your password.";
    return "";
  },

  // Sign up needs a strong password
  newPassword(value) {
    if (value === "") return "Create a password.";
    const checks = passwordChecks(value);
    if (!checks.length || !checks.upper || !checks.lower || !checks.number) {
      return "Password doesn't meet all the requirements yet.";
    }
    return "";
  },

  confirmPassword(value) {
    const original = document.getElementById("password").value;
    if (value === "") return "Re-enter your password.";
    if (value !== original) return "Passwords don't match.";
    return "";
  },

  terms(checked) {
    return checked ? "" : "You need to accept the terms to continue.";
  }
};

// ----- Helpers -----

function passwordChecks(pw) {
  return {
    length: pw.length >= 8,
    upper: /[A-Z]/.test(pw),
    lower: /[a-z]/.test(pw),
    number: /[0-9]/.test(pw),
    special: /[^A-Za-z0-9]/.test(pw)
  };
}

// Run the rule for one input and show/hide its error message
function validateField(input) {
  const field = input.closest(".field");
  const ruleName = input.dataset.rule;
  const value = input.type === "checkbox" ? input.checked : input.value;
  const message = rules[ruleName](value);

  field.querySelector(".error").textContent = message;
  field.classList.toggle("invalid", message !== "");
  field.classList.toggle("valid", message === "" && input.type !== "checkbox" && input.value !== "");
  return message === "";
}

// Validate every field in a form. Returns true only if all pass.
function validateForm(form) {
  let allGood = true;
  let firstBad = null;
  form.querySelectorAll("[data-rule]").forEach(function (input) {
    if (!validateField(input)) {
      allGood = false;
      if (!firstBad) firstBad = input;
    }
  });
  if (firstBad) firstBad.focus();
  return allGood;
}

// ----- Wire everything up -----

const form = document.querySelector("form");
const submitBtn = document.getElementById("submitBtn");

form.querySelectorAll("[data-rule]").forEach(function (input) {
  // Show errors after the user leaves a field...
  input.addEventListener("blur", function () {
    if (input.value !== "" || input.type === "checkbox") validateField(input);
  });
  // ...and keep updating live once a field has already been flagged
  const liveEvent = input.type === "checkbox" ? "change" : "input";
  input.addEventListener(liveEvent, function () {
    const field = input.closest(".field");
    if (field.classList.contains("invalid") || field.classList.contains("valid")) validateField(input);
  });
});

// Show / hide password buttons
document.querySelectorAll(".toggle").forEach(function (btn) {
  btn.addEventListener("click", function () {
    const input = btn.parentElement.querySelector("input");
    const hidden = input.type === "password";
    input.type = hidden ? "text" : "password";
    btn.textContent = hidden ? "Hide" : "Show";
  });
});

// Sign up page extras: live checklist + strength meter
const pwInput = document.getElementById("password");
const meterBar = document.getElementById("meterBar");
if (pwInput && meterBar) {
  const labels = ["", "Weak", "Weak", "Fair", "Good", "Strong"];
  const colors = ["#d85535", "#d85535", "#d85535", "#e8863a", "#ffb037", "#ffb037"];

  pwInput.addEventListener("input", function () {
    const checks = passwordChecks(pwInput.value);
    let score = 0;
    for (const key in checks) {
      const item = document.querySelector('[data-check="' + key + '"]');
      if (item) item.classList.toggle("ok", checks[key]);
      if (checks[key]) score++;
    }
    if (pwInput.value === "") score = 0;

    meterBar.style.width = (score / 5) * 100 + "%";
    meterBar.style.background = colors[score];
    meterBar.style.boxShadow = score >= 4 ? "0 0 10px rgba(255,176,55,.7)" : "none";
    document.getElementById("meterLabel").textContent = labels[score];

    // If the confirm box was already filled, re-check that it still matches
    const confirm = document.getElementById("confirm");
    if (confirm.value !== "") validateField(confirm);
  });
}

// Submit
// The browser-side checks run first. If everything passes we let the form post to the server,
// which checks the same rules again (never trust the browser alone).
form.addEventListener("submit", function (event) {
  if (!validateForm(form)) {
    event.preventDefault();
    form.classList.remove("shake");
    void form.offsetWidth;            // restarts the animation
    form.classList.add("shake");
    return;
  }

  submitBtn.classList.add("loading");
  submitBtn.disabled = true;          // stops double clicks while the page posts
});

// If the user presses the Back button, reset the button
window.addEventListener("pageshow", function () {
  submitBtn.classList.remove("loading");
  submitBtn.disabled = false;
});

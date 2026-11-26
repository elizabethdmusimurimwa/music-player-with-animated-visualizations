// Scripts for the admin pages:
//   1. Live form validation (same style as auth.js: each input has a data-rule attribute)
//   2. Confirm dialog for delete / demote buttons
//   3. Toast messages, count-up numbers and small helpers (previews, counters)
// The server checks the same rules again, so nothing here replaces server-side validation.

document.addEventListener("DOMContentLoaded", function () {
  "use strict";

  const AUDIO_TYPES = [".mp3", ".wav", ".ogg", ".m4a"];
  const IMAGE_TYPES = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
  const PATH_PATTERN = /^(~\/|https?:\/\/).+/;

  
  //  1. Validation rules
  //  A rule returns "" when the value is fine, or an error message.

  // Checks the file chosen in a <input type="file">
  function checkFile(input, allowed, maxMb) {
    if (input.files.length === 0) return "";
    const file = input.files[0];
    const dot = file.name.lastIndexOf(".");
    const extension = dot >= 0 ? file.name.slice(dot).toLowerCase() : "";

    if (allowed.indexOf(extension) === -1) return "Allowed types: " + allowed.join(", ") + ".";
    if (file.size > maxMb * 1024 * 1024) return "File is too large (max " + maxMb + " MB).";
    return "";
  }

  const rules = {
    songTitle(value) {
      value = value.trim();
      if (value === "") return "Enter the song title.";
      if (value.length > 100) return "Title can be 100 characters at most.";
      return "";
    },

    artistName(value) {
      value = value.trim();
      if (value === "") return "Enter the artist's name.";
      if (value.length < 2) return "Name needs at least 2 characters.";
      if (value.length > 100) return "Name can be 100 characters at most.";
      return "";
    },

    genreName(value) {
      value = value.trim();
      if (value === "") return "Enter the genre name.";
      if (value.length < 2) return "Genre name needs at least 2 characters.";
      if (value.length > 40) return "Genre name can be 40 characters at most.";
      if (!/^[A-Za-z0-9][A-Za-z0-9 &'\-\/]*$/.test(value)) return "Use letters, numbers, spaces and & ' - / only.";
      return "";
    },

    album(value) {
      if (value.trim().length > 100) return "Album can be 100 characters at most.";
      return "";
    },

    biography(value) {
      if (value.length > 2000) return "Biography can be 2000 characters at most.";
      return "";
    },

    // A drop-down that must have something chosen 
    select(value, input) {
      if (value === "") return input.dataset.message || "Choose an option.";
      return "";
    },

    // Optional path or link
    pathOrUrl(value) {
      value = value.trim();
      if (value === "") return "";
      if (value.length > 500) return "Path is too long (500 characters at most).";
      if (!PATH_PATTERN.test(value)) return "Start the path with ~/ or the link with http:// or https://";
      return "";
    },

    // Audio needs an uploaded file or a path
    audioPath(value) {
      value = value.trim();
      const fileInput = document.getElementById("audioFile");
      const hasFile = fileInput && fileInput.files.length > 0;

      if (value === "") {
        return hasFile ? "" : "Upload an audio file or enter a path or URL.";
      }
      return rules.pathOrUrl(value);
    },

    audioFile(value, input) {
      return checkFile(input, AUDIO_TYPES, 25);
    },

    imageFile(value, input) {
      return checkFile(input, IMAGE_TYPES, 3);
    },

    duration(value) {
      value = value.trim();
      if (value === "") return "";
      if (!/^\d+$/.test(value)) return "Enter whole seconds, like 226.";
      if (Number(value) > 7200) return "Enter a length between 0 and 7200 seconds.";
      return "";
    }
  };

  // Helpers

  // Runs the rule for one input and shows / hides its error message
  function validateField(input) {
    const field = input.closest(".field");
    const rule = rules[input.dataset.rule];
    const message = rule(input.value, input);

    field.querySelector(".error").textContent = message;
    field.classList.toggle("invalid", message !== "");
    field.classList.toggle("valid", message === "" && input.value !== "");
    return message === "";
  }

  // Validates every field of a form. Returns true only if all pass.
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

  // Wire up the entity forms (song / artist / genre) 

  const form = document.querySelector("form.form-grid");

  if (form) {
    const submitBtn = document.getElementById("submitBtn");

    form.querySelectorAll("[data-rule]").forEach(function (input) {
      // Show the error after the user leaves the field
      input.addEventListener("blur", function () {
        if (input.type === "file" || input.value !== "") validateField(input);
      });

      // keep it updated live once the field has been flagged
      const liveEvent = (input.tagName === "SELECT" || input.type === "file") ? "change" : "input";
      input.addEventListener(liveEvent, function () {
        const field = input.closest(".field");
        if (input.type === "file" || field.classList.contains("invalid") || field.classList.contains("valid")) {
          validateField(input);
        }
        // A file can satisfy the "audio source" rule of another field
        if (input.dataset.recheck) {
          const other = document.getElementById(input.dataset.recheck);
          if (other && (input.type === "file" || other.value !== "")) validateField(other);
        }
      });
    });

    form.addEventListener("submit", function (event) {
      if (!validateForm(form)) {
        event.preventDefault();
        form.classList.remove("shake");
        void form.offsetWidth;          // restarts the animation
        form.classList.add("shake");
        return;
      }
      submitBtn.classList.add("loading");
      submitBtn.disabled = true;        // stops double clicks while the page posts
    });

    // If the user comes back with the Back button, reset the button
    window.addEventListener("pageshow", function () {
      if (submitBtn) {
        submitBtn.classList.remove("loading");
        submitBtn.disabled = false;
      }
    });

    setUpCounters();
    setUpPreviews(form);
    setUpDuration();
    setUpGenreDemo();
  }

  //  Small helpers used by the forms

  // "12 / 2000" character counters
  function setUpCounters() {
    [["bio", "bioCounter", 2000], ["genreName", "nameCounter", 40]].forEach(function (pair) {
      const input = document.getElementById(pair[0]);
      const counter = document.getElementById(pair[1]);
      if (!input || !counter) return;

      function update() {
        counter.textContent = input.value.length + " / " + pair[2];
        counter.classList.toggle("warn", input.value.length >= pair[2] * 0.9);
      }
      input.addEventListener("input", update);
      update();
    });
  }

  // Turns "~/Content/Images/a.jpg" into a URL the browser can open
  function toUrl(path, root) {
    path = path.trim();
    if (path.indexOf("~/") === 0) return root + path.slice(2);
    return path;
  }

  // Live cover image + audio previews
  function setUpPreviews(formEl) {
    const root = formEl.dataset.root || "/";
    const imageFile = document.getElementById("imageFile");
    const imagePath = document.getElementById("imagePath");
    const coverBox = document.getElementById("coverBox");
    const coverImg = document.getElementById("coverPreview");
    const coverEmpty = document.getElementById("coverEmpty");

    function showCover(src) {
      if (!coverImg) return;
      if (src) {
        coverImg.src = src;
        coverImg.hidden = false;
        coverEmpty.hidden = true;
        coverBox.classList.add("has-image");
      } else {
        coverImg.hidden = true;
        coverEmpty.hidden = false;
        coverBox.classList.remove("has-image");
      }
    }

    function refreshCover() {
      if (imageFile && imageFile.files.length > 0 && checkFile(imageFile, IMAGE_TYPES, 3) === "") {
        showCover(URL.createObjectURL(imageFile.files[0]));
      } else if (imagePath && PATH_PATTERN.test(imagePath.value.trim())) {
        showCover(toUrl(imagePath.value, root));
      } else {
        showCover("");
      }
    }

    if (coverImg) {
      coverImg.addEventListener("error", function () { showCover(""); });   // broken link
      if (imageFile) imageFile.addEventListener("change", refreshCover);
      if (imagePath) imagePath.addEventListener("input", refreshCover);
      refreshCover();
    }

    // Audio preview (songs only)
    const audioFile = document.getElementById("audioFile");
    const audioPath = document.getElementById("audioPath");
    const player = document.getElementById("audioPreview");
    const hint = document.getElementById("audioHint");

    function refreshAudio() {
      if (!player) return;
      let src = "";

      if (audioFile.files.length > 0 && checkFile(audioFile, AUDIO_TYPES, 25) === "") {
        src = URL.createObjectURL(audioFile.files[0]);
      } else if (PATH_PATTERN.test(audioPath.value.trim())) {
        src = toUrl(audioPath.value, root);
      }

      if (src) {
        player.src = src;
        hint.textContent = "Ready to play.";
      } else {
        player.removeAttribute("src");
        player.load();
        hint.textContent = "Choose a file or enter a link to listen before saving.";
      }
    }

    if (player) {
      audioFile.addEventListener("change", refreshAudio);
      audioPath.addEventListener("change", refreshAudio);
      refreshAudio();
    }
  }

  // Shows 226 as 3:46 and fills the length in automatically for uploaded audio
  function setUpDuration() {
    const duration = document.getElementById("duration");
    const preview = document.getElementById("durationPreview");
    const audioFile = document.getElementById("audioFile");
    if (!duration || !preview) return;

    function update() {
      const seconds = parseInt(duration.value, 10);
      if (isNaN(seconds) || seconds < 0) {
        preview.textContent = "0:00";
        return;
      }
      preview.textContent = Math.floor(seconds / 60) + ":" + String(seconds % 60).padStart(2, "0");
    }
    duration.addEventListener("input", update);
    update();

    if (audioFile) {
      audioFile.addEventListener("change", function () {
        if (audioFile.files.length === 0 || checkFile(audioFile, AUDIO_TYPES, 25) !== "") return;

        // A hidden <audio> element can read the length of the chosen file
        const probe = new Audio();
        probe.preload = "metadata";
        probe.src = URL.createObjectURL(audioFile.files[0]);
        probe.addEventListener("loadedmetadata", function () {
          if (isFinite(probe.duration)) {
            duration.value = Math.round(probe.duration);
            update();
            validateField(duration);
          }
        });
      });
    }
  }

  // Genre page: the preview tile follows what is typed
  function setUpGenreDemo() {
    const input = document.getElementById("genreName");
    const demo = document.getElementById("genreDemo");
    if (!input || !demo) return;

    input.addEventListener("input", function () {
      demo.textContent = input.value.trim() === "" ? "Your genre" : input.value.trim();
    });
  }
  //  2. Confirm dialog for buttons with data-post
  //  <button data-post="/AdminSongs/Delete/5" data-confirm-title="..." data-confirm-text="...">
  //  Without data-confirm-title the button posts straight away.
  

  const modal = document.getElementById("confirmModal");
  const postForm = document.getElementById("postForm");
  let pendingUrl = "";
  let lastFocus = null;

  function openConfirm(button) {
    pendingUrl = button.dataset.post;
    lastFocus = button;
    document.getElementById("confirmTitle").textContent = button.dataset.confirmTitle;
    document.getElementById("confirmText").textContent = button.dataset.confirmText || "";
    document.getElementById("confirmOk").textContent = button.dataset.confirmButton || "Yes, delete";
    modal.hidden = false;
    document.getElementById("confirmCancel").focus();
  }

  function closeConfirm() {
    modal.hidden = true;
    pendingUrl = "";
    if (lastFocus) lastFocus.focus();
  }

  function sendPost(url) {
    postForm.action = url;
    postForm.submit();
  }

  document.querySelectorAll("[data-post]").forEach(function (button) {
    button.addEventListener("click", function () {
      if (button.dataset.confirmTitle) {
        openConfirm(button);
      } else {
        button.disabled = true;
        sendPost(button.dataset.post);
      }
    });
  });

  if (modal) {
    document.getElementById("confirmCancel").addEventListener("click", closeConfirm);
    document.getElementById("confirmOk").addEventListener("click", function () {
      const url = pendingUrl;
      this.classList.add("loading");
      this.disabled = true;
      sendPost(url);
    });
    // Click outside the box, or press Esc, to cancel
    modal.addEventListener("click", function (event) {
      if (event.target === modal) closeConfirm();
    });
    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape" && !modal.hidden) closeConfirm();
    });
  }

  // Filter drop-downs reload the list as soon as they change
  document.querySelectorAll("[data-autosubmit]").forEach(function (select) {
    select.addEventListener("change", function () {
      select.closest("form").submit();
    });
  });

  //  3. Toasts, count-up numbers, mobile menu

  document.querySelectorAll(".toast").forEach(function (toast, index) {
    setTimeout(function () { toast.classList.add("show"); }, 150 + index * 120);
    setTimeout(function () {
      toast.classList.remove("show");
      setTimeout(function () { toast.remove(); }, 500);
    }, 5200 + index * 120);
    toast.addEventListener("click", function () { toast.remove(); });
  });

  document.querySelectorAll("[data-count]").forEach(function (el) {
    const target = parseInt(el.dataset.count, 10) || 0;
    const reduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (reduced || target === 0) {
      el.textContent = target;
      return;
    }

    const duration = 900;
    const start = performance.now();
    function tick(now) {
      const progress = Math.min((now - start) / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3);        // starts fast, slows down
      el.textContent = Math.round(target * eased);
      if (progress < 1) requestAnimationFrame(tick);
    }
    requestAnimationFrame(tick);
  });

  const menuBtn = document.getElementById("menuBtn");
  const scrim = document.getElementById("scrim");
  if (menuBtn) {
    menuBtn.addEventListener("click", function () { document.body.classList.toggle("nav-open"); });
    scrim.addEventListener("click", function () { document.body.classList.remove("nav-open"); });
  }
});

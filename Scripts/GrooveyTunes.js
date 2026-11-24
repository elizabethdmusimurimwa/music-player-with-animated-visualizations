// @ts-nocheck
document.addEventListener("DOMContentLoaded", function () {
    "use strict";
    const $ = (id) => document.getElementById(id);

    const audio = $("audioPlayer");
    if (!audio) return;

    // ---- Bottom bar ----
    const playBtn = $("playBtn"), previousBtn = $("previousBtn"), nextBtn = $("nextBtn");
    const currentSong = $("currentSong"), currentArtist = $("currentArtist");
    const progressBar = $("progressBar"), currentTime = $("currentTime"), duration = $("duration");
    const volume = $("volume"), volumeIcon = $("volumeIcon");
    const playerAnimation = $("playerAnimation");

    // ---- Expanded player ----
    const pageBody = $("pageBody"), panel = $("expandedPlayer");
    const bigCover = $("bigCover"), bigTitle = $("bigTitle"), bigArtist = $("bigArtist");
    const bigSeek = $("bigSeek"), bigPlay = $("bigPlay");
    const bigCurrent = $("bigCurrent"), bigTotal = $("bigTotal");
    const modal = $("coverModal"), gallery = $("animGallery");

    function formatTime(seconds) {
        if (!Number.isFinite(seconds)) return "0:00";
        const m = Math.floor(seconds / 60);
        const s = Math.floor(seconds % 60);
        return m + ":" + String(s).padStart(2, "0");
    }

    function setInfo(title, artist) {
        currentSong.textContent = title;
        currentArtist.textContent = artist;
        bigTitle.textContent = title;
        bigArtist.textContent = artist;
    }

    // =========================================================
    //  Song queue
    //  The queue is remembered, so Next / Previous keep working
    //  even after you move to a different page.
    // =========================================================
    let queue = [];          // [{ title, artist, audio }]
    let currentIndex = -1;

    function toItem(button) {
        return {
            title: button.dataset.title || "Unknown title",
            artist: button.dataset.artist || "",
            audio: button.dataset.audio || ""
        };
    }

    function readQueueFromPage() {
        return Array.from(document.querySelectorAll(".play-song")).map(toItem);
    }

    // Highlight the row of the song that is playing (if it is on this page)
    function highlightPlaying() {
        const cur = queue[currentIndex];
        document.querySelectorAll(".play-song").forEach(function (b) {
            const row = b.closest("tr");
            if (row) row.classList.toggle("playing", !!cur && b.dataset.audio === cur.audio);
        });
    }

    function loadSong(index) {
        if (queue.length === 0) queue = readQueueFromPage();
        if (queue.length === 0) return;

        index = (index + queue.length) % queue.length;
        const song = queue[index];

        if (!song.audio) {
            alert("Audio file not available for this song.");
            return;
        }

        currentIndex = index;
        audio.src = song.audio;
        setInfo(song.title, song.artist);
        highlightPlaying();

        progressBar.value = 0;
        bigSeek.value = 0;
        currentTime.textContent = bigCurrent.textContent = "0:00";
        duration.textContent = bigTotal.textContent = "0:00";

        audio.play().catch(function (err) {
            if (err && err.name === "AbortError") return;   // switched songs quickly
            syncPlayIcons();
            alert("Unable to play this audio file. Check its path.");
        });
    }

    // Clicking a play button builds the queue from the songs on the current page.
    // (Delegated, so it keeps working after a page is swapped in.)
    document.addEventListener("click", function (e) {
        const btn = e.target.closest(".play-song");
        if (!btn) return;
        const buttons = Array.from(document.querySelectorAll(".play-song"));
        queue = buttons.map(toItem);
        loadSong(buttons.indexOf(btn));
    });

    // Double-clicking a row plays it
    document.addEventListener("dblclick", function (e) {
        if (e.target.closest(".play-btn")) return;
        const row = e.target.closest("tr");
        const btn = row && row.querySelector(".play-song");
        if (btn) btn.click();
    });

    // =========================================================
    //  Play / pause, previous, next
    // =========================================================
    function togglePlay() {
        if (!audio.getAttribute("src")) {
            loadSong(0);
            return;
        }
        if (audio.paused) {
            audio.play().catch(function () { });
        } else {
            audio.pause();
        }
    }

    function syncPlayIcons() {
        const icon = audio.paused ? "▶" : "⏸";
        const label = audio.paused ? "Play" : "Pause";
        playBtn.textContent = bigPlay.textContent = icon;
        playBtn.title = bigPlay.title = label;
    }

    function goPrevious() {
        // Past 3 seconds: restart the song. Otherwise go to the previous one.
        if (audio.currentTime > 3) { audio.currentTime = 0; return; }
        loadSong(currentIndex - 1);
    }
    function goNext() { loadSong(currentIndex + 1); }

    playBtn.addEventListener("click", togglePlay);
    bigPlay.addEventListener("click", togglePlay);
    previousBtn.addEventListener("click", goPrevious);
    nextBtn.addEventListener("click", goNext);
    $("bigPrev").addEventListener("click", goPrevious);
    $("bigNext").addEventListener("click", goNext);

    audio.addEventListener("play", syncPlayIcons);
    audio.addEventListener("pause", syncPlayIcons);
    audio.addEventListener("ended", goNext);

    // =========================================================
    //  Progress and time
    // =========================================================
    function syncTime() {
        const d = audio.duration;
        const has = Number.isFinite(d) && d > 0;
        const pct = has ? (audio.currentTime / d) * 100 : 0;

        progressBar.value = pct;
        bigSeek.value = pct;
        currentTime.textContent = bigCurrent.textContent = formatTime(audio.currentTime);
        duration.textContent = bigTotal.textContent = formatTime(d);
    }

    function seekFrom(input) {
        const d = audio.duration;
        if (Number.isFinite(d) && d > 0) audio.currentTime = (input.value / 100) * d;
    }

    audio.addEventListener("timeupdate", syncTime);
    audio.addEventListener("loadedmetadata", syncTime);
    audio.addEventListener("durationchange", syncTime);
    progressBar.addEventListener("input", function () { seekFrom(progressBar); });
    bigSeek.addEventListener("input", function () { seekFrom(bigSeek); });

    // =========================================================
    //  Volume (remembered, with mute)
    // =========================================================
    const VOL_KEY = "groovey.volume";
    let startVolume = 0.7;
    try {
        const saved = parseFloat(localStorage.getItem(VOL_KEY));
        if (Number.isFinite(saved)) startVolume = Math.min(1, Math.max(0, saved));
    } catch (e) { }

    audio.volume = startVolume;
    volume.value = startVolume;

    volume.addEventListener("input", function () {
        audio.volume = parseFloat(volume.value);
        audio.muted = audio.volume === 0;
        try { localStorage.setItem(VOL_KEY, String(audio.volume)); } catch (e) { }
    });

    volumeIcon.addEventListener("click", function () { audio.muted = !audio.muted; });

    audio.addEventListener("volumechange", function () {
        const silent = audio.muted || audio.volume === 0;
        volumeIcon.textContent = silent ? "🔇" : (audio.volume < 0.5 ? "🔉" : "🔊");
        volumeIcon.title = silent ? "Unmute" : "Mute";
        volume.value = audio.muted ? 0 : audio.volume;
    });

    // =========================================================
    //  Expanded player open / close
    // =========================================================
    function showPlayer() {
        pageBody.hidden = true;
        panel.hidden = false;
        setInfo(currentSong.textContent, currentArtist.textContent);
        syncTime();
        syncPlayIcons();
    }
    function hidePlayer() {
        closeModal();
        panel.hidden = true;
        pageBody.hidden = false;
    }

    $("openPlayer").addEventListener("click", function (e) { e.preventDefault(); showPlayer(); });
    $("closePlayer").addEventListener("click", hidePlayer);

    // =========================================================
    //  Animations: the user can only choose from this list.
    //  The first one is the default.
    // =========================================================
    const COVER_KEY = "groovey.animation";

    const ANIMATIONS = [
        {
            name: "King Julien",
            url: "https://media.tenor.com/Lh9WGnxYz2wAAAAM/trampoline-king-julien.gif"
        },
        {
            name: "Spongebob",
            url: "https://media1.tenor.com/m/NMFzSC2hCpsAAAAd/spongebob-squarepants-dance.gif"
        },
        {
            name: "Sailor Moon",
            url: "https://media1.giphy.com/media/v1.Y2lkPTc5MGI3NjExdjl2dnd4NzMzYXYwcWYxNmhzcHNqMHhweWRsaHlmOWkzNTVxd216bSZlcD12MV9naWZzX3NlYXJjaCZjdD1n/TCzhA2n5sBY5y/200.webp"
        }
    ];

    function isAllowed(url) {
        return ANIMATIONS.some(function (a) { return a.url === url; });
    }

    // Some sites refuse images when a referrer is sent, so don't send one
    playerAnimation.referrerPolicy = "no-referrer";
    bigCover.referrerPolicy = "no-referrer";

    let currentAnimation = ANIMATIONS[0].url;
    try {
        const saved = localStorage.getItem(COVER_KEY);
        if (saved && isAllowed(saved)) currentAnimation = saved;
    } catch (e) { }

    function applyAnimation() {
        playerAnimation.src = currentAnimation;
        bigCover.src = currentAnimation;
    }

    function markSelected() {
        Array.prototype.forEach.call(gallery.querySelectorAll("img"), function (img) {
            img.classList.toggle("selected", img.getAttribute("data-url") === currentAnimation);
        });
    }

    function chooseAnimation(url) {
        currentAnimation = url;
        try { localStorage.setItem(COVER_KEY, url); } catch (e) { }
        applyAnimation();
        markSelected();
        closeModal();
    }

    function openModal() { modal.hidden = false; markSelected(); }
    function closeModal() { modal.hidden = true; }

    ANIMATIONS.forEach(function (a) {
        const img = document.createElement("img");
        img.referrerPolicy = "no-referrer";
        img.src = a.url;
        img.alt = a.name;
        img.title = a.name;
        img.setAttribute("data-url", a.url);
        img.addEventListener("click", function () { chooseAnimation(a.url); });
        img.addEventListener("error", function () {
            console.warn("Animation could not be loaded:", a.name, a.url);
            img.remove();
        });
        gallery.appendChild(img);
    });

    // Clicking the big animation opens the chooser
    $("coverWrap").addEventListener("click", openModal);
    $("closeModal").addEventListener("click", closeModal);
    modal.addEventListener("click", function (e) { if (e.target === modal) closeModal(); });
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && !modal.hidden) closeModal();
    });

    // =========================================================
    //  Page navigation WITHOUT reloading, so the music keeps playing.
    //  Only the main area is swapped; the player stays alive.
    // =========================================================
    const mainScroll = document.querySelector(".main-content");

    // Pages that use a different layout need a normal full page load
    const FULL_LOAD = /^\/(admin|account)/i;

    function isInPageLink(a) {
        if (!a || !a.getAttribute("href")) return false;
        if (a.target && a.target !== "_self") return false;
        if (a.hasAttribute("download")) return false;
        if (a.getAttribute("href") === "#") return false;

        const url = new URL(a.href, location.href);
        if (url.origin !== location.origin) return false;
        if (url.pathname === location.pathname && url.search === location.search && url.hash) return false;
        if (FULL_LOAD.test(url.pathname)) return false;
        return true;
    }

    function addNewStyles(doc) {
        doc.querySelectorAll('link[rel="stylesheet"]').forEach(function (link) {
            const path = new URL(link.href, location.href).pathname;
            const exists = Array.from(document.querySelectorAll('link[rel="stylesheet"]')).some(function (l) {
                return new URL(l.href, location.href).pathname === path;
            });
            if (!exists) document.head.appendChild(link.cloneNode(true));
        });
    }

    function runPageScripts(doc) {
        doc.body.querySelectorAll("script").forEach(function (old) {
            const src = old.getAttribute("src");
            if (src && /GrooveyTunes\.js/i.test(src)) return;    // never load this file twice

            if (src) {
                const already = Array.from(document.scripts).some(function (s) {
                    return s.getAttribute("src") === src;
                });
                if (already) return;
            }

            const s = document.createElement("script");
            if (src) s.src = src; else s.textContent = old.textContent;
            document.body.appendChild(s);
            if (!src) s.remove();
        });
    }

    function syncChrome(doc) {
        // Highlighted icons
        const sel = ".topbar a.icon-tile, .sidebar a.icon-tile";
        const now = document.querySelectorAll(sel);
        const next = doc.querySelectorAll(sel);
        now.forEach(function (a, i) {
            if (next[i]) a.classList.toggle("active", next[i].classList.contains("active"));
        });

        // Search box text
        const mine = document.querySelector(".search");
        const theirs = doc.querySelector(".search");
        if (mine && theirs) mine.value = theirs.getAttribute("value") || "";
    }

    async function navigate(url, push) {
        try {
            const res = await fetch(url, {
                headers: { "X-Requested-With": "XMLHttpRequest" },
                credentials: "same-origin"
            });
            const type = res.headers.get("content-type") || "";
            if (!res.ok || type.indexOf("text/html") < 0) { location.href = url; return; }

            const doc = new DOMParser().parseFromString(await res.text(), "text/html");
            const newBody = doc.getElementById("pageBody");

            // Not our layout (admin, login...) -> do a normal page load
            if (!newBody || !doc.getElementById("audioPlayer")) { location.href = url; return; }

            hidePlayer();
            addNewStyles(doc);
            pageBody.innerHTML = newBody.innerHTML;
            document.title = doc.title;
            syncChrome(doc);

            if (push) history.pushState({ gt: true }, "", res.url || url);

            runPageScripts(doc);
            highlightPlaying();
            if (mainScroll) mainScroll.scrollTop = 0;
        } catch (err) {
            location.href = url;
        }
    }

    // Sidebar / header / in-page links
    document.addEventListener("click", function (e) {
        if (e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
        const a = e.target.closest("a");
        if (!a || !a.closest(".sidebar, .topbar, #pageBody")) return;
        if (!isInPageLink(a)) return;
        e.preventDefault();
        navigate(a.href, true);
    });

    // Search box
    document.addEventListener("submit", function (e) {
        const form = e.target;
        if (!form.matches(".search-wrap")) return;
        e.preventDefault();
        const params = new URLSearchParams(new FormData(form)).toString();
        navigate(form.action + (params ? "?" + params : ""), true);
    });

    // Back / forward buttons
    window.addEventListener("popstate", function () { navigate(location.href, false); });

    // =========================================================
    //  Start-up
    // =========================================================
    applyAnimation();
    syncTime();
    syncPlayIcons();
});
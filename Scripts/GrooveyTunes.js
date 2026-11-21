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
    //  Song queue (every .play-song button on the page)
    // =========================================================
    const songs = Array.from(document.querySelectorAll(".play-song"));
    let currentIndex = -1;

    function loadSong(index) {
        if (songs.length === 0) return;

        index = (index + songs.length) % songs.length;
        const song = songs[index];
        const src = song.dataset.audio;

        if (!src) {
            alert("Audio file not available for this song.");
            return;
        }

        currentIndex = index;

        // Highlight the playing row
        songs.forEach(function (b) {
            const row = b.closest("tr");
            if (row) row.classList.toggle("playing", b === song);
        });

        audio.src = src;
        setInfo(song.dataset.title || "Unknown title", song.dataset.artist || "");

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

    songs.forEach(function (button, index) {
        button.addEventListener("click", function () { loadSong(index); });
        const row = button.closest("tr");
        if (row) row.addEventListener("dblclick", function () { loadSong(index); });
    });

    // =========================================================
    //  Play / pause, previous, next
    // =========================================================
    function togglePlay() {
        if (!audio.getAttribute("src")) {
            if (songs.length > 0) loadSong(0);
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
            url: "https://media1.tenor.com/m/NMFzSC2hCpsAAAAC/spongebob-squarepants-dance.gif"
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
    //  Start-up
    // =========================================================
    applyAnimation();
    syncTime();
    syncPlayIcons();
});
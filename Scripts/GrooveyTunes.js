
document.addEventListener("DOMContentLoaded", function () {

    const audioPlayer = document.getElementById("audioPlayer");
    const playBtn = document.getElementById("playBtn");
    const previousBtn = document.getElementById("previousBtn");
    const nextBtn = document.getElementById("nextBtn");

    const currentSong = document.getElementById("currentSong");
    const currentArtist = document.getElementById("currentArtist");
    const progressBar = document.getElementById("progressBar");
    const currentTime = document.getElementById("currentTime");
    const duration = document.getElementById("duration");
    const volume = document.getElementById("volume");

    const songButtons = document.querySelectorAll(".play-song");

    let songs = Array.from(songButtons);
    let currentIndex = 0;

    function formatTime(seconds) {
        if (!Number.isFinite(seconds)) return "0:00";

        const minutes = Math.floor(seconds / 60);
        const remainingSeconds = Math.floor(seconds % 60);

        return minutes + ":" + String(remainingSeconds).padStart(2, "0");
    }

    function loadSong(index) {
        if (songs.length === 0) return;

        currentIndex = (index + songs.length) % songs.length;

        const song = songs[currentIndex];

        const title = song.dataset.title;
        const artist = song.dataset.artist;
        const audio = song.dataset.audio;
        const image = song.dataset.image;

        if (!audio) {
            alert("Audio file not available for this song.");
            return;
        }

        audioPlayer.src = audio;
        currentSong.textContent = title;
        currentArtist.textContent = artist;

        const animation = document.getElementById("playerAnimation");

        if (animation && image) {
            animation.src = image;
        }

        progressBar.value = 0;
        currentTime.textContent = "0:00";
        duration.textContent = "0:00";

        audioPlayer.play()
            .then(() => {
                playBtn.textContent = "⏸";
            })
            .catch(() => {
                playBtn.textContent = "▶";
                alert("Unable to play this audio file. Check its path.");
            });
    }

    songButtons.forEach((button, index) => {
        button.addEventListener("click", function () {
            loadSong(index);
        });
    });

    playBtn.addEventListener("click", function () {
        if (!audioPlayer.src) {
            if (songs.length > 0) loadSong(0);
            return;
        }

        if (audioPlayer.paused) {
            audioPlayer.play();
            playBtn.textContent = "⏸";
        } else {
            audioPlayer.pause();
            playBtn.textContent = "▶";
        }
    });

    previousBtn.addEventListener("click", function () {
        loadSong(currentIndex - 1);
    });

    nextBtn.addEventListener("click", function () {
        loadSong(currentIndex + 1);
    });

    audioPlayer.addEventListener("timeupdate", function () {
        if (audioPlayer.duration) {
            progressBar.value =
                (audioPlayer.currentTime / audioPlayer.duration) * 100;

            currentTime.textContent = formatTime(audioPlayer.currentTime);
        }
    });

    audioPlayer.addEventListener("loadedmetadata", function () {
        duration.textContent = formatTime(audioPlayer.duration);
    });

    audioPlayer.addEventListener("ended", function () {
        loadSong(currentIndex + 1);
    });

    progressBar.addEventListener("input", function () {
        if (audioPlayer.duration) {
            audioPlayer.currentTime =
                (progressBar.value / 100) * audioPlayer.duration;
        }
    });

    volume.addEventListener("input", function () {
        audioPlayer.volume = volume.value;
    });

    audioPlayer.volume = volume.value;
});

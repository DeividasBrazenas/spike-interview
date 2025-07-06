let audioContext, analyser, micStream, mediaRecorder, recordedChunks = [];
let silenceStart = null, speaking = false, micBlocked = false;

const silenceThreshold = 0.01;
const silenceDelayMs = 1000;

async function startListening(callback) {
    micStream = await navigator.mediaDevices.getUserMedia({ audio: true });
    audioContext = new AudioContext();
    const source = audioContext.createMediaStreamSource(micStream);
    analyser = audioContext.createAnalyser();
    analyser.fftSize = 512;
    source.connect(analyser);
    monitorMic(callback);
}

function stopListening() {
    if (mediaRecorder && mediaRecorder.state !== "inactive") mediaRecorder.stop();
    micStream?.getTracks().forEach(track => track.stop());
    audioContext?.close();
    speaking = false;
    micBlocked = false;
}

function monitorMic(callback) {
    const data = new Uint8Array(analyser.fftSize);
    const loop = () => {
        if (!window.isCallActive || micBlocked) return requestAnimationFrame(loop);
        analyser.getByteTimeDomainData(data);
        const rms = Math.sqrt(data.reduce((s, v) => s + (v - 128) ** 2, 0) / data.length);
        const volume = rms / 128;
        const now = Date.now();
        if (volume > silenceThreshold) {
            if (!speaking) {
                speaking = true;
                startRecording(callback);
            }
            silenceStart = null;
        } else if (speaking) {
            if (!silenceStart) silenceStart = now;
            if (now - silenceStart > silenceDelayMs) {
                stopRecording();
                speaking = false;
                silenceStart = null;
            }
        }
        requestAnimationFrame(loop);
    };
    loop();
}

function startRecording(callback) {
    recordedChunks = [];
    mediaRecorder = new MediaRecorder(micStream, { mimeType: "audio/webm" });
    mediaRecorder.ondataavailable = e => {
        if (e.data.size > 0) recordedChunks.push(e.data);
    };
    mediaRecorder.onstop = async () => {
        const blob = new Blob(recordedChunks, { type: "audio/webm" });
        const buffer = await blob.arrayBuffer();
        const byteArray = Array.from(new Uint8Array(buffer));
        micBlocked = true;
        await callback(byteArray);
    };
    mediaRecorder.start();
}

function stopRecording() {
    if (mediaRecorder && mediaRecorder.state !== "inactive") mediaRecorder.stop();
}

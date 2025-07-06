const player = document.getElementById("player");
let callId;
window.isCallActive = false;
let latestError = null;
let isProcessing = false;
let isEnding = false;

function getFormPatientValues() {
    return {
        FirstName: document.getElementById("firstName").value,
        LastName: document.getElementById("lastName").value,
        DateOfBirth: document.getElementById("dob").value,
        MemberId: document.getElementById("memberId").value,
        InsuranceActiveAsOf: new Date(document.getElementById("activeDate").value).toISOString(),
        DateOfTreatment: new Date(document.getElementById("treatmentDate").value).toISOString()
    };
}

document.getElementById("startCallBtn").onclick = async () => {
    const patient = getFormPatientValues()
    const selectedVoiceId = document.getElementById("voiceSelect").value;
    
    latestError = null;

    await setupConnection({
        onAudio: byteArray => {
            if (!byteArray?.length) {
                micBlocked = false;
                return;
            }

            const blob = new Blob([new Uint8Array(byteArray)], {type: "audio/mpeg"});
            player.src = URL.createObjectURL(blob);
            player.play();
            setStatus("🔊", "Responding...");
        },
        onMessage: msg => setStatus("💬", msg),
        onError: err => {
            latestError = err;
            setStatus("❌", `${err.Type}: ${err.Message}`);
            micBlocked = false;
        },
        onSummary: updateCallSummary
    });

    try {
        setPatientFormEnabled(false);
        
        const request = {Patient: patient, VoiceId: selectedVoiceId};
        callId = await invoke("StartCall", request);

        if (latestError) throw latestError;

        setStatus("📞", "Call started, listening...");
        window.isCallActive = true;
        startListening(sendAudio);
        document.getElementById("startCallBtn").disabled = true;
        document.getElementById("endCallBtn").disabled = false;
    } catch (err) {
        console.error("StartCall failed", err);
        setPatientFormEnabled(true);
        setStatus("❌", `Failed to start call: ${err?.Message || err?.message || "Unexpected error"}`);
        window.isCallActive = false;
        callId = null;
        await stopConnection();
    }
};

document.getElementById("endCallBtn").onclick = async () => {
    if (!callId || !window.isCallActive || isEnding) return;

    isEnding = true;
    window.isCallActive = false;

    // Wait for any in-flight audio processing
    while (isProcessing) {
        await new Promise(resolve => setTimeout(resolve, 100));
    }

    try {
        await invoke("EndCall", { CallId: callId });
        setStatus("🔚", "Call ended");
    } catch (e) {
        setStatus("❌", `Error ending call: ${e?.Message || e?.message || e}`);
    }

    stopListening();
    setPatientFormEnabled(true);
    document.getElementById("startCallBtn").disabled = false;
    document.getElementById("endCallBtn").disabled = true;

    await stopConnection();
    callId = null;
    isEnding = false;
};

async function sendAudio(byteArray) {
    // Cancel sending if call is over or in transition
    if (!window.isCallActive || isProcessing || !callId) {
        return;
    }

    isProcessing = true;
    setStatus("⏳", "Processing...");

    try {
        await invoke("Interact", callId, byteArray);
    } catch (err) {
        const message = err?.Message || err?.message || String(err);

        if (
            message.includes("Invocation canceled") ||
            message.includes("connection being closed") ||
            message.includes("connection was terminated")
        ) {
            console.warn("📴 Audio send skipped: connection was closed during processing.");
        } else {
            console.error("❌ Audio send failed:", err);
            setStatus("❌", `Audio send failed: ${message}`);
        }
    } finally {
        isProcessing = false;
    }
}

player.onended = () => {
    micBlocked = false;
    if (window.isCallActive) {
        setStatus("🎤", "Listening...");
        startListening(sendAudio);
    }
    isProcessing = false;
};

prefillDates();

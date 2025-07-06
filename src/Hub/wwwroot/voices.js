const voices = [
    { voice_id: "FGY2WhTYpPnrIDTdsKH5", name: "Laura", style: "American, sassy, social media" },
    { voice_id: "9BWtsMINqrJLrRacOk9x", name: "Aria", style: "American, husky, educational" },
    { voice_id: "EXAVITQu4vr4xnSDxMaL", name: "Sarah", style: "American, young, TV" },
    { voice_id: "IKne3meq5aSn9XLyUdCD", name: "Charlie", style: "Australian, hyped, conversational" },
    { voice_id: "JBFqnCBsd6RMkjVDRZzb", name: "George", style: "British, mature, storytelling" },
    { voice_id: "N2lVS1w4EtoT3dr4eOWO", name: "Callum", style: "English, characters, mid-aged" },
    { voice_id: "SAz9YHcvj6GT2YYXdXww", name: "River", style: "Neutral, calm, conversational" },
    { voice_id: "TX3LPaxmHKxFdv7VOQHJ", name: "Liam", style: "American, confident, casual" },
    { voice_id: "XB0fDUnXU5powFXDhCwa", name: "Charlotte", style: "Swedish, animated, relaxed" },
    { voice_id: "Xb7hH8MSUJpSbSDYk0k2", name: "Alice", style: "British, professional, ad" }
];

function populateVoiceDropdown() {
    const select = document.getElementById("voiceSelect");
    voices.forEach(voice => {
        const option = document.createElement("option");
        option.value = voice.voice_id;
        option.textContent = `${voice.name} (${voice.style})`;
        select.appendChild(option);
    });
}

// Run on load
populateVoiceDropdown();

function setStatus(icon, text) {
    document.getElementById("statusIcon").textContent = icon;
    document.getElementById("statusText").textContent = text;
}

function prefillDates() {
    const now = new Date().toISOString().slice(0, 16);
    document.getElementById("activeDate").value = now;
    document.getElementById("treatmentDate").value = now;
}

function setPatientFormEnabled(enabled) {
    document.querySelectorAll("#inputForm input").forEach(i => i.disabled = !enabled);
    document.getElementById("voiceSelect").disabled = !enabled;
}

function updateCallSummary(summary) {
    document.getElementById("summaryTable").style.display = "table";
    document.getElementById("refNum").textContent = summary.ReferenceNumber;
    document.getElementById("visitLimit").textContent = summary.VisitLimit;
    document.getElementById("visitLimitStructure").textContent = summary.VisitLimitStructure;
    document.getElementById("visitsUsed").textContent = summary.VisitsUsed;
    document.getElementById("copay").textContent = summary.Copay;
    document.getElementById("deductible").textContent = summary.Deductible;
    document.getElementById("deductibleMet").textContent = summary.DeductibleMet;
    document.getElementById("oopMax").textContent = summary.OutOfPocketMaximum;
    document.getElementById("oopMet").textContent = summary.OutOfPocketMet;
    document.getElementById("authRequired").textContent = summary.InitialAuthorizationRequired;
}

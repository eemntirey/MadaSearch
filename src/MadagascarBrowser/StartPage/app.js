(function () {
    var timeEl = document.getElementById("time");
    var dateEl = document.getElementById("date");
    var searchForm = document.getElementById("searchForm");
    var searchInput = document.getElementById("q");
    var fmtTime = new Intl.DateTimeFormat("fr-FR", {
        timeZone: "Indian/Antananarivo",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit"
    });
    var fmtDate = new Intl.DateTimeFormat("fr-FR", {
        timeZone: "Indian/Antananarivo",
        weekday: "long",
        day: "numeric",
        month: "long",
        year: "numeric"
    });

    function tick() {
        var now = new Date();
        timeEl.textContent = fmtTime.format(now);
        dateEl.textContent = fmtDate.format(now);
    }

    tick();
    setInterval(tick, 1000);

    if (searchForm && searchInput) {
        searchForm.addEventListener("submit", function (event) {
            event.preventDefault();
            var query = searchInput.value.trim();
            if (query) {
                var bridge = window.chrome && window.chrome.webview;
                if (bridge) {
                    bridge.postMessage("search:" + encodeURIComponent(query));
                } else {
                    searchForm.submit();
                }
            }
        });
    }
})();

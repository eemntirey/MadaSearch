#!/usr/bin/env python3
"""Serveur HTTP de test deterministe pour les parcours manuels phase 1.

Port 8777. Pages :
  /                  page d'accueil de test (liens cible, window.open, images)
  /target-blank      lien target=_blank
  /window-open       bouton window.open()
  /download          reponse Content-Disposition: attachment
  /download-named    Content-Disposition avec nom de fichier
  /big-image.jpg     image 2 Mo (DataSaver)
  /video.mp4         reponse Content-Type video/mp4 (DataSaver)
  /ads/ads.js        script pub (AdBlock)
  /error404          status 404
  /error500          status 500
  /exempt.example    (resolu via /exempt) pas de blocage
"""
from http.server import BaseHTTPRequestHandler, HTTPServer
import os
import struct
import zlib

PORT = int(os.environ.get("TESTSERVER_PORT", "8777"))
COUNTER = {"n": 0}

def make_jpeg(size_bytes: int) -> bytes:
    """JPEG minimal (entetes + corps padding) de la taille indiquee."""
    soi = b"\xff\xd8\xff\xe0\x00\x10JFIF\x00\x01\x01\x00\x00\x01\x00\x01\x00\x00"
    eoi = b"\xff\xd9"
    pad = size_bytes - len(soi) - len(eoi)
    if pad < 0:
        pad = 0
    # Entete JPEG de segment comment pour remplir sans casser le format
    comment = b"\xff\xfe" + struct.pack(">H", min(pad + 2, 65535))
    body = comment + b"X" * max(0, pad - len(comment))
    return soi + body[:pad] + eoi

BIG_JPEG = make_jpeg(2_000_000)   # 2 Mo : au-dessus du seuil image
SMALL_JPEG = make_jpeg(8_000)     # 8 Ko : en-dessous du seuil

HOME = """<!DOCTYPE html>
<html lang="fr"><head><meta charset="utf-8"><title>Page de test</title></head>
<body>
<h1>Page de test Madagascar</h1>
<p id="status">pret</p>
<a id="blankLink" href="/target-blank" target="_blank">Lien target blank</a>
<a id="windowLink" href="#" onclick="window.open('/target-blank');return false;">window.open</a>
<a id="navLink" href="/page2">Page 2</a>
<img id="bigImg" src="/big-image.jpg" width="10" height="10" alt="grande">
<img id="smallImg" src="/small-image.jpg" width="10" height="10" alt="petite">
<a id="dlLink" href="/download-named">Telechargement</a>
<script src="/ads/ads.js"></script>
<script>document.getElementById('bigImg').src='/big-image.jpg?cb='+Date.now();</script>
<div id="find-target">aiguille dans la botte de foin</div>
<div id="find-target2">aiguille encore ici</div>
</body></html>"""

TARGET_BLANK = """<!DOCTYPE html>
<html lang="fr"><head><meta charset="utf-8"><title>Nouvel onglet cible</title></head>
<body><h1>Nouvel onglet cible</h1></body></html>"""

PAGE2 = """<!DOCTYPE html>
<html lang="fr"><head><meta charset="utf-8"><title>Page 2</title></head>
<body><h1>Page 2</h1><a href="/">Retour</a></body></html>"""

class Handler(BaseHTTPRequestHandler):
    def log_message(self, fmt, *args):
        pass  # silencieux

    def _send(self, status: int, ctype: str, body: bytes, extra_headers=None):
        self.send_response(status)
        self.send_header("Content-Type", ctype)
        self.send_header("Content-Length", str(len(body)))
        if extra_headers:
            for k, v in extra_headers:
                self.send_header(k, v)
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        path = self.path.split("?")[0]
        if path == "/":
            self._send(200, "text/html; charset=utf-8", HOME.encode())
        elif path == "/target-blank":
            self._send(200, "text/html; charset=utf-8", TARGET_BLANK.encode())
        elif path == "/page2":
            self._send(200, "text/html; charset=utf-8", PAGE2.encode())
        elif path == "/big-image.jpg":
            self._send(200, "image/jpeg", BIG_JPEG)
        elif path == "/small-image.jpg":
            self._send(200, "image/jpeg", SMALL_JPEG)
        elif path == "/video.mp4":
            self._send(200, "video/mp4", b"\x00\x00\x00\x18ftypmp42" + b"\x00" * 3_000_000)
        elif path in ("/download", "/download-named"):
            payload = b"contenu de test\r\nligne 2\r\n" * 100
            self._send(200, "application/octet-stream", payload,
                       extra_headers=[("Content-Disposition", 'attachment; filename="rapport-test.txt"')])
        elif path == "/ads/ads.js":
            self._send(200, "application/javascript", b"console.log('pub');")
        elif path == "/error404":
            self._send(404, "text/html; charset=utf-8", b"<h1>404</h1>")
        elif path == "/error500":
            self._send(500, "text/html; charset=utf-8", b"<h1>500</h1>")
        elif path == "/counter":
            COUNTER["n"] += 1
            body = ("<html lang=\"fr\"><head><meta charset=\"utf-8\"><title>Compteur</title></head>"
                    "<body><h1>Compteur: %d</h1><p>Chaque chargement incremente.</p>"
                    "</body></html>" % COUNTER["n"]).encode()
            self._send(200, "text/html; charset=utf-8", body)
        elif path == "/input":
            body = (b"<!DOCTYPE html><html lang=\"fr\"><head><meta charset=\"utf-8\"><title>Saisie</title></head>"
                    b"<body><h1 id=\"echo\">(vide)</h1>"
                    b"<input id=\"q\" autofocus oninput=\"document.getElementById('echo').textContent=this.value\">"
                    b"</body></html>")
            self._send(200, "text/html; charset=utf-8", body)
        elif path == "/long":
            body = (b"<!DOCTYPE html><html lang=\"fr\"><head><meta charset=\"utf-8\"><title>Longue</title></head>"
                    b"<body onscroll=\"document.title='scroll:'+window.scrollY\">"
                    b"<h1>Page longue</h1>" + b"<p>bloc de test</p>" * 400 + b"</body></html>")
            self._send(200, "text/html; charset=utf-8", body)
        else:
            self._send(404, "text/html; charset=utf-8", b"<h1>404</h1>")

if __name__ == "__main__":
    HTTPServer(("127.0.0.1", PORT), Handler).serve_forever()

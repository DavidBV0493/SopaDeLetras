console.log("INICIO JUEGO.JS");

// ── Estado del juego ───────────────────────────────────────
let juegoIniciado = false;
let segundos = 0;
let intervaloTimer = 0;

// ── Iniciar juego ──────────────────────────────────────────
function iniciarJuego() {
    if (juegoIniciado) return;
    juegoIniciado = true;
    document.getElementById("btn-iniciar").disabled = true;
    document.getElementById("btn-iniciar").textContent = "▶ EN JUEGO";
    document.getElementById("tablero").classList.remove("tablero-bloqueado");
    intervaloTimer = setInterval(() => {
        segundos++;
        document.getElementById("timer").textContent = segundos;
    }, 1000);
}

// ── Finalizar juego (timer + bloqueo sin borrar colores) ───
function finalizarJuego() {
    clearInterval(intervaloTimer)
    juegoIniciado = false;
    const tablero = document.getElementById("tablero");
    tablero.style.pointerEvents = "none"; // Bloquea interacciones sin borrar colores
    tablero.style.opacity = "0.75"; // Opcional: visualmente indica que el juego terminó
}

// ── Variables de selección ─────────────────────────────────
let seleccionando = false;
let celdasSeleccionadas = [];
let direccionFijada = null;
const tableroEl = document.getElementById("tablero");

// ── Eventos del tablero ────────────────────────────────────
tableroEl.addEventListener("mousedown", e => {
    if (!juegoIniciado) return;
    const td = e.target.closest("td");
    if (!td) return;
    seleccionando = true;
    celdasSeleccionadas = [];
    direccionFijada = null;
    limpiarSeleccion();
    agregarCelda(td);
});

tableroEl.addEventListener("mousemove", e => {
    if (!seleccionando) return;
    const td = e.target.closest("td");
    if (!td) return;
    agregarCelda(td);
});

document.addEventListener("mouseup", () => {
    if (!seleccionando) return;
    seleccionando = false;
    verificarPalabra();
});

// ── Helpers ────────────────────────────────────────────────
function agregarCelda(td) {
    const f = parseInt(td.dataset.fila);
    const c = parseInt(td.dataset.col);
    if (celdasSeleccionadas.some(s => s.f === f && s.c === c)) return;
    // celdasSeleccionadas.push({ f, c, td });
    // td.style.backgroundColor = "#ffe066";
    if (celdasSeleccionadas.length === 0) {
        celdasSeleccionadas.push({ f, c, td });
        td.style.backgroundColor = "#ffe066";
        return;
    }
    if (celdasSeleccionadas.length === 1) {
        const primera = celdasSeleccionadas[0];
        const df = Math.sign(f - primera.f);
        const dc = Math.sign(c - primera.c);
        if (df === 0 && dc === 0) return;
        direccionFijada = { df, dc };
        celdasSeleccionadas.push({ f, c, td });
        td.style.backgroundColor = "#ffe066";
        return;
    }
    if (!direccionFijada) return;
    const ultima = celdasSeleccionadas[celdasSeleccionadas.length - 1];
    const dfActual = Math.sign(f - ultima.f);
    const dcActual = Math.sign(c - ultima.c);
    if (dfActual !== direccionFijada.df || dcActual !== direccionFijada.dc) return;
    const fEsperada = ultima.f + direccionFijada.df;
    const cEsperada = ultima.c + direccionFijada.dc;
    if (f !== fEsperada || c !== cEsperada) return;
    celdasSeleccionadas.push({ f, c, td });
    td.style.backgroundColor = "#ffe066";
}

function limpiarSeleccion() {
    celdasSeleccionadas.forEach(s => s.td.style.backgroundColor = "");
    document.querySelectorAll("td.seleccionada")
        .forEach(td => {
            td.classList.remove("seleccionada");
            td.style.backgroundColor = "";
        });
}

// ── Verificar palabra ──────────────────────────────────────
function verificarPalabra() {
    if (celdasSeleccionadas.length === 0) return;
    const letras = celdasSeleccionadas.map(s => s.td.textContent.trim()).join("");
    const letrasInverso = letras.split("").reverse().join("");
    const match = palabras.find(p =>
        p.texto === letras || p.texto === letrasInverso);
    if (match) {
        celdasSeleccionadas.forEach(s => {
            s.td.style.backgroundColor = "#90ee90"; // ← verde inline
        });
        marcarEncontrada(match.texto);
        const todasEncontradas = palabras.every(p =>
            document.getElementById(`pal-${p.texto}`)?.classList.contains("tachada"));
        if (todasEncontradas) {
            finalizarJuego();
            alert(`🎉 ¡Felicidades! Encontraste todas las palabras en ${segundos} segundos.`);
        }
    } else {
        limpiarSeleccion();
    }
    celdasSeleccionadas = [];
}

// ── Marcar palabra en la lista ─────────────────────────────
function marcarEncontrada(texto) {
    const tag = document.getElementById(`pal-${texto}`);
    if (tag) tag.classList.add("tachada");
}

// ── RESOLVER ───────────────────────────────────────────────
function resolver() {
    palabras.forEach(p => {
        const [df, dc] = p.dir.split(",").map(Number);
        for (let i = 0; i < p.texto.length; i++) {
            const f = p.fila + df * i;
            const c = p.col + dc * i;
            const td = document.querySelector(`td[data-fila="${f}"][data-col="${c}"]`);
            if (td) td.style.backgroundColor = "#ff9999"; // ← rojo inline
        }
        marcarEncontrada(p.texto);
    });
    finalizarJuego();
}

// ── REINICIAR JUEGO ────────────────────────────────────────
function reiniciarJuego() {
    window.location.href = "/";
}
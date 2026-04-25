/* ═══════════════════════════════════════════════════════════════
   TWIX JS — particles, scroll reveal, modals, interactions
   ═══════════════════════════════════════════════════════════════ */

// ── Particle canvas ───────────────────────────────────────────────
(function () {
    const canvas = document.getElementById('bg-canvas');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    let W, H, particles = [], mouse = { x: 0, y: 0 };

    function resize() {
        W = canvas.width = window.innerWidth;
        H = canvas.height = window.innerHeight;
        mouse.x = W / 2;
        mouse.y = H / 2;
    }

    function rand(a, b) { return Math.random() * (b - a) + a; }

    function makeParticle() {
        const big = Math.random() < 0.1;
        return {
            x: rand(0, W), y: rand(0, H),
            r: big ? rand(2, 4) : rand(0.4, 1.8),
            dx: rand(-0.3, 0.3),
            dy: rand(-0.5, -0.1),
            alpha: rand(0.06, 0.5),
            da: rand(-0.003, 0.003),
            big,
            hue: rand(20, 40), // orange range
        };
    }

    function init() {
        particles = Array.from({ length: 70 }, makeParticle);
    }

    window.addEventListener('mousemove', e => { mouse.x = e.clientX; mouse.y = e.clientY; });

    function frame() {
        ctx.clearRect(0, 0, W, H);

        // Mouse glow
        const mg = ctx.createRadialGradient(mouse.x, mouse.y, 0, mouse.x, mouse.y, 350);
        mg.addColorStop(0, 'rgba(255,140,30,0.05)');
        mg.addColorStop(1, 'rgba(0,0,0,0)');
        ctx.fillStyle = mg;
        ctx.fillRect(0, 0, W, H);

        particles.forEach(p => {
            // Gentle mouse attraction
            const ex = (mouse.x / W - 0.5) * 0.12;
            const ey = (mouse.y / H - 0.5) * 0.06;
            p.x += p.dx + ex;
            p.y += p.dy + ey;
            p.alpha += p.da;

            if (p.alpha > 0.55) p.da = -Math.abs(p.da);
            if (p.alpha < 0.02) p.da = Math.abs(p.da);
            if (p.y < -10) { p.y = H + 10; p.x = rand(0, W); }
            if (p.x < -10) p.x = W + 10;
            if (p.x > W + 10) p.x = -10;

            ctx.save();
            if (p.big) {
                // Glowing orb
                const g = ctx.createRadialGradient(p.x, p.y, 0, p.x, p.y, p.r * 20);
                g.addColorStop(0, `hsla(${p.hue},100%,60%,${p.alpha * 0.9})`);
                g.addColorStop(0.4, `hsla(${p.hue},100%,50%,${p.alpha * 0.3})`);
                g.addColorStop(1, 'transparent');
                ctx.fillStyle = g;
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.r * 20, 0, Math.PI * 2);
                ctx.fill();
            } else {
                // Crisp dot with soft glow
                ctx.globalAlpha = p.alpha;
                ctx.shadowBlur = 6;
                ctx.shadowColor = `hsl(${p.hue},100%,60%)`;
                ctx.fillStyle = `hsl(${p.hue},100%,65%)`;
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
                ctx.fill();
            }
            ctx.restore();
        });

        // Connection lines
        for (let i = 0; i < particles.length; i++) {
            for (let j = i + 1; j < particles.length; j++) {
                const a = particles[i], b = particles[j];
                const d = Math.hypot(a.x - b.x, a.y - b.y);
                if (d < 110) {
                    ctx.save();
                    ctx.globalAlpha = (1 - d / 110) * 0.07;
                    ctx.strokeStyle = '#ff9933';
                    ctx.lineWidth = 0.6;
                    ctx.beginPath();
                    ctx.moveTo(a.x, a.y);
                    ctx.lineTo(b.x, b.y);
                    ctx.stroke();
                    ctx.restore();
                }
            }
        }

        requestAnimationFrame(frame);
    }

    window.addEventListener('resize', () => { resize(); init(); });
    resize(); init(); frame();
})();

// ── Scroll reveal ─────────────────────────────────────────────────
(function () {
    const io = new IntersectionObserver(entries => {
        entries.forEach(e => {
            if (e.isIntersecting) { e.target.classList.add('revealed'); io.unobserve(e.target); }
        });
    }, { threshold: 0.07 });

    function setup() {
        document.querySelectorAll(
            '.stat-card, .admin-section, .admin-card, .profile-info, .compose-box, .feed-tabs, .panel-card'
        ).forEach(el => { el.classList.add('reveal'); io.observe(el); });
    }

    document.readyState === 'loading'
        ? document.addEventListener('DOMContentLoaded', setup)
        : setup();
})();

// ── Char counter ──────────────────────────────────────────────────
function updateCount(textarea) {
    const el = document.getElementById('char-count');
    if (!el) return;
    const left = 280 - textarea.value.length;
    el.textContent = left;
    el.style.color = left < 20 ? '#e74c3c' : left < 50 ? '#ff9933' : '';
}

// ── Edit modal ────────────────────────────────────────────────────
function openEdit(postId, content) {
    const modal = document.getElementById('edit-modal');
    if (!modal) return;
    document.getElementById('edit-post-id').value = postId;
    const ta = document.getElementById('edit-content');
    ta.value = content;
    modal.style.display = 'flex';
    ta.focus();
    ta.setSelectionRange(ta.value.length, ta.value.length);
}
function closeEdit() {
    const m = document.getElementById('edit-modal');
    if (m) m.style.display = 'none';
}
document.addEventListener('click', e => {
    const m = document.getElementById('edit-modal');
    if (m && e.target === m) closeEdit();
});
document.addEventListener('keydown', e => { if (e.key === 'Escape') closeEdit(); });

// ── Button pop on click ───────────────────────────────────────────
document.addEventListener('click', e => {
    const btn = e.target.closest('.action-btn');
    if (!btn) return;
    btn.style.transform = 'scale(1.4)';
    setTimeout(() => btn.style.transform = '', 180);
});

// ── Auto-dismiss alerts ───────────────────────────────────────────
document.querySelectorAll('.alert').forEach(a => {
    setTimeout(() => {
        a.style.transition = 'opacity .5s, max-height .5s, padding .5s';
        a.style.opacity = '0'; a.style.maxHeight = '0'; a.style.padding = '0';
        setTimeout(() => a.remove(), 500);
    }, 3500);
});

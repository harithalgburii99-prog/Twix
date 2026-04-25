/* ═══════════════════════════════════════════════════════════════
   TWIX JS — particles + scroll reveal + UI interactions
   ═══════════════════════════════════════════════════════════════ */

// ── Particle background ──────────────────────────────────────────
(function () {
    const canvas = document.getElementById('bg-canvas');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    let W, H, particles = [];

    const PARTICLE_COUNT = 55;
    const COLORS = [
        'rgba(255,153,51,',
        'rgba(255,180,80,',
        'rgba(200,100,20,',
        'rgba(255,120,40,',
    ];

    function resize() {
        W = canvas.width = window.innerWidth;
        H = canvas.height = window.innerHeight;
    }

    function rand(min, max) { return Math.random() * (max - min) + min; }

    function createParticle() {
        return {
            x: rand(0, W),
            y: rand(0, H),
            r: rand(0.5, 2.2),
            dx: rand(-0.25, 0.25),
            dy: rand(-0.4, -0.08),
            alpha: rand(0.08, 0.45),
            da: rand(-0.002, 0.002),
            color: COLORS[Math.floor(Math.random() * COLORS.length)],
            // occasional big glowing orbs
            glow: Math.random() < 0.12,
        };
    }

    function initParticles() {
        particles = [];
        for (let i = 0; i < PARTICLE_COUNT; i++) particles.push(createParticle());
    }

    // Subtle mouse parallax
    let mx = W / 2, my = H / 2;
    window.addEventListener('mousemove', e => { mx = e.clientX; my = e.clientY; });

    function draw() {
        ctx.clearRect(0, 0, W, H);

        // Very subtle radial glow following mouse
        const grad = ctx.createRadialGradient(mx, my, 0, mx, my, Math.max(W, H) * 0.55);
        grad.addColorStop(0, 'rgba(255,153,51,0.04)');
        grad.addColorStop(1, 'rgba(0,0,0,0)');
        ctx.fillStyle = grad;
        ctx.fillRect(0, 0, W, H);

        particles.forEach((p, i) => {
            // Move
            p.x += p.dx + (mx / W - 0.5) * 0.08;
            p.y += p.dy;
            p.alpha += p.da;

            // Clamp alpha
            if (p.alpha > 0.5) { p.da = -Math.abs(p.da); }
            if (p.alpha < 0.03) { p.da = Math.abs(p.da); }

            // Wrap around
            if (p.y < -10) { p.y = H + 10; p.x = rand(0, W); }
            if (p.x < -10) { p.x = W + 10; }
            if (p.x > W + 10) { p.x = -10; }

            // Draw
            ctx.save();
            if (p.glow) {
                // Big soft orb
                const g = ctx.createRadialGradient(p.x, p.y, 0, p.x, p.y, p.r * 18);
                g.addColorStop(0, p.color + (p.alpha * 0.8) + ')');
                g.addColorStop(1, p.color + '0)');
                ctx.fillStyle = g;
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.r * 18, 0, Math.PI * 2);
                ctx.fill();
            } else {
                // Small crisp dot
                ctx.globalAlpha = p.alpha;
                ctx.fillStyle = p.color + '1)';
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
                ctx.fill();
            }
            ctx.restore();
        });

        // Draw subtle connecting lines between nearby particles
        for (let i = 0; i < particles.length; i++) {
            for (let j = i + 1; j < particles.length; j++) {
                const a = particles[i], b = particles[j];
                const dist = Math.hypot(a.x - b.x, a.y - b.y);
                if (dist < 100) {
                    ctx.save();
                    ctx.globalAlpha = (1 - dist / 100) * 0.06;
                    ctx.strokeStyle = 'rgba(255,153,51,1)';
                    ctx.lineWidth = 0.5;
                    ctx.beginPath();
                    ctx.moveTo(a.x, a.y);
                    ctx.lineTo(b.x, b.y);
                    ctx.stroke();
                    ctx.restore();
                }
            }
        }

        requestAnimationFrame(draw);
    }

    window.addEventListener('resize', () => { resize(); initParticles(); });
    resize();
    initParticles();
    draw();
})();

// ── Scroll reveal ────────────────────────────────────────────────
(function () {
    // Mark elements for reveal
    const selectors = [
        '.post-card',
        '.stat-card',
        '.admin-section',
        '.admin-card',
        '.profile-info',
        '.compose-box',
        '.feed-tabs',
    ];

    function addRevealClass() {
        selectors.forEach(sel => {
            document.querySelectorAll(sel).forEach(el => {
                // Don't double-add
                if (!el.classList.contains('post-card')) { // post-cards use CSS animation
                    el.classList.add('reveal');
                }
            });
        });
    }

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('revealed');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.08 });

    function observeAll() {
        document.querySelectorAll('.reveal').forEach(el => observer.observe(el));
    }

    // Run after DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => { addRevealClass(); observeAll(); });
    } else {
        addRevealClass();
        observeAll();
    }
})();

// ── Char counter ─────────────────────────────────────────────────
function updateCount(textarea) {
    const el = document.getElementById('char-count');
    if (!el) return;
    const remaining = 280 - textarea.value.length;
    el.textContent = remaining;
    el.style.color = remaining < 20 ? '#e74c3c' : remaining < 50 ? '#ff9933' : '';
}

// ── Edit modal ───────────────────────────────────────────────────
function openEdit(postId, content) {
    const modal = document.getElementById('edit-modal');
    const idInput = document.getElementById('edit-post-id');
    const textarea = document.getElementById('edit-content');
    if (!modal || !idInput || !textarea) return;
    idInput.value = postId;
    textarea.value = content;
    modal.style.display = 'flex';
    textarea.focus();
    textarea.setSelectionRange(textarea.value.length, textarea.value.length);
}

function closeEdit() {
    const modal = document.getElementById('edit-modal');
    if (modal) modal.style.display = 'none';
}

// Close on backdrop click
document.addEventListener('click', e => {
    const modal = document.getElementById('edit-modal');
    if (modal && e.target === modal) closeEdit();
});

// Close on Escape
document.addEventListener('keydown', e => {
    if (e.key === 'Escape') closeEdit();
});

// ── Like button pop animation ─────────────────────────────────────
document.addEventListener('click', e => {
    const btn = e.target.closest('.action-btn');
    if (!btn) return;
    btn.style.transform = 'scale(1.35)';
    setTimeout(() => { btn.style.transform = ''; }, 180);
});

// ── Auto-dismiss alerts ───────────────────────────────────────────
document.querySelectorAll('.alert').forEach(alert => {
    setTimeout(() => {
        alert.style.transition = 'opacity .5s, max-height .5s';
        alert.style.opacity = '0';
        alert.style.maxHeight = '0';
        setTimeout(() => alert.remove(), 500);
    }, 3500);
});

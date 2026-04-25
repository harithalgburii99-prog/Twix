// ── Char counter ─────────────────────────────────────────────────
function updateCount(textarea) {
  const el = document.getElementById('char-count');
  if (el) el.textContent = 280 - textarea.value.length;
}

// ── Edit modal ───────────────────────────────────────────────────
function openEdit(postId, content) {
  document.getElementById('edit-post-id').value = postId;
  document.getElementById('edit-content').value  = content;
  document.getElementById('edit-modal').style.display = 'flex';
  document.getElementById('edit-content').focus();
}

function closeEdit() {
  document.getElementById('edit-modal').style.display = 'none';
}

// Close modal on backdrop click
document.addEventListener('click', function (e) {
  const modal = document.getElementById('edit-modal');
  if (modal && e.target === modal) closeEdit();
});

// Close modal on Escape
document.addEventListener('keydown', function (e) {
  if (e.key === 'Escape') closeEdit();
});

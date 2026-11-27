using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// Reusable card board: shows picture cards in a centred grid and returns
    /// either one pick (choice) or a multi selection (Level 3 boards).
    /// Layout is computed manually so it always fits, whatever the card count.
    /// </summary>
    [DisallowMultipleComponent]
    public class CardPanel : MonoBehaviour
    {
        [Header("Wiring")]
        public CanvasGroup group;
        public RectTransform grid;
        public CardView cardPrefab;
        public Text titleText;
        public Button confirmButton;
        public Text confirmLabel;
        public Button skipButton;
        public Text skipLabel;
        public Button extraButton;
        public Text extraLabel;
        public Button closeButton;

        [Header("Layout")]
        public float gap = 26f;
        public int maxColumns = 4;
        public float maxCardWidth = 330f;

        readonly List<CardView> cards = new List<CardView>();
        Action<CardView> onPick;
        Action<List<CardData>> onConfirm;
        Action onSkip;
        Action onExtra;
        Action<CardView> onToggle;
        bool multi;
        bool allowEmptyConfirm = true;
        int maxSelect;

        public List<CardData> Selection { get; } = new List<CardData>();
        public bool IsOpen => group != null && group.alpha > 0.01f;

        void Awake()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }
            if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmClicked);
            if (skipButton != null) skipButton.onClick.AddListener(SkipClicked);
            if (extraButton != null) extraButton.onClick.AddListener(ExtraClicked);
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() =>
                {
                    AudioManager.I?.PlayClick();
                    Hide();
                });
            }
        }

        void Update()
        {
            // keep the selection frame readable when the screen is resized
            if (IsOpen && multi) RefreshConfirmState();
        }

        // ------------------------------------------------------------------ api

        public void ShowSingle(IList<CardData> items, Action<CardView> picked, string title = null)
        {
            multi = false;
            maxSelect = 1;
            onPick = picked;
            onConfirm = null;
            onSkip = null;
            onExtra = null;
            onToggle = null;
            allowEmptyConfirm = false;

            Build(items, title);
            if (confirmButton != null) confirmButton.gameObject.SetActive(false);
            if (skipButton != null) skipButton.gameObject.SetActive(false);
            if (extraButton != null) extraButton.gameObject.SetActive(false);
            Open();
        }

        public void ShowMulti(IList<CardData> items, Action<List<CardData>> confirmed,
            IList<CardData> preselect = null, Action skipped = null, string title = null,
            Action<CardView> toggled = null, int selectionLimit = 0)
        {
            multi = true;
            maxSelect = selectionLimit;
            allowEmptyConfirm = true;
            onPick = null;
            onConfirm = confirmed;
            onSkip = skipped;
            onExtra = null;
            onToggle = toggled;

            Build(items, title);

            Selection.Clear();
            if (preselect != null)
            {
                foreach (var p in preselect)
                {
                    var card = cards.Find(c => c.Data != null && c.Data.id == p.id);
                    if (card != null)
                    {
                        card.SetSelected(true);
                        Selection.Add(card.Data);
                    }
                }
            }

            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(true);
                if (confirmLabel != null) confirmLabel.text = "Continue";
            }
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(skipped != null);
                if (skipLabel != null) skipLabel.text = "Skip";
            }
            if (extraButton != null) extraButton.gameObject.SetActive(false);
            RefreshConfirmState();
            Open();
        }

        /// <summary>
        /// Fully configurable board used by the Feelings Board: single or multi select,
        /// custom footer buttons and an optional third action.
        /// </summary>
        public void ShowSelection(IList<CardData> items, bool allowMulti, int selectionLimit,
            string title, string confirmText, string skipText, string extraText,
            Action<List<CardData>> confirmed, Action skipped, Action extra,
            IList<CardData> preselect = null)
        {
            multi = true;                        // selection handling is the same
            maxSelect = allowMulti ? selectionLimit : 1;
            allowEmptyConfirm = true;
            onPick = null;
            onConfirm = confirmed;
            onSkip = skipped;
            onExtra = extra;
            onToggle = null;

            Build(items, title);

            Selection.Clear();
            if (preselect != null)
            {
                foreach (var p in preselect)
                {
                    var card = cards.Find(c => c.Data != null && c.Data.id == p.id);
                    if (card != null)
                    {
                        card.SetSelected(true);
                        Selection.Add(card.Data);
                    }
                }
            }

            if (confirmButton != null)
            {
                confirmButton.gameObject.SetActive(true);
                if (confirmLabel != null) confirmLabel.text = string.IsNullOrEmpty(confirmText) ? "Continue" : confirmText;
            }
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(skipped != null);
                if (skipLabel != null) skipLabel.text = string.IsNullOrEmpty(skipText) ? "Skip" : skipText;
            }
            if (extraButton != null)
            {
                extraButton.gameObject.SetActive(extra != null);
                if (extraLabel != null) extraLabel.text = extraText;
            }
            RefreshConfirmState();
            Open();
        }

        void Open()
        {
            if (group == null) return;
            StopAllCoroutines();
            group.blocksRaycasts = true;
            group.interactable = true;
            StartCoroutine(OpenRoutine());
        }

        IEnumerator OpenRoutine()
        {
            yield return UIFx.FadeGroup(group, group.alpha, 1f, 0.2f);
            for (int i = 0; i < cards.Count; i++)
                StartCoroutine(cards[i].PopIn(i * 0.05f));
        }

        public void Hide()
        {
            if (group == null) return;
            group.blocksRaycasts = false;
            group.interactable = false;
            if (!gameObject.activeInHierarchy)
            {
                group.alpha = 0f;
                return;
            }
            StopAllCoroutines();
            StartCoroutine(UIFx.FadeGroup(group, group.alpha, 0f, 0.16f));
        }

        public void HideImmediate()
        {
            if (group == null) return;
            StopAllCoroutines();
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }

        // -------------------------------------------------------------- building

        void Build(IList<CardData> items, string title)
        {
            foreach (var c in cards)
                if (c != null) Destroy(c.gameObject);
            cards.Clear();

            if (titleText != null)
            {
                titleText.text = string.IsNullOrEmpty(title) ? "" : title;
                titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
            }

            if (items == null || items.Count == 0) return;

            for (int i = 0; i < items.Count; i++)
            {
                var card = Instantiate(cardPrefab, grid);
                card.name = "Card_" + (items[i].id ?? i.ToString());
                card.Bind(items[i], i);
                card.Clicked += HandleCardClicked;
                cards.Add(card);
            }

            Layout();
        }

        void Layout()
        {
            int n = cards.Count;
            if (n == 0 || grid == null) return;

            int cols = Mathf.Clamp(n, 1, maxColumns);
            int rows = Mathf.CeilToInt(n / (float)cols);

            float availW = grid.rect.width;
            float availH = grid.rect.height;
            if (availW <= 1f) availW = 1600f;
            if (availH <= 1f) availH = 420f;

            float cw = (availW - gap * (cols - 1)) / cols;
            float ch = (availH - gap * (rows - 1)) / rows;
            cw = Mathf.Min(cw, maxCardWidth);
            ch = Mathf.Min(ch, cw * 1.28f);
            ch = Mathf.Min(ch, availH);

            float totalW = cw * cols + gap * (cols - 1);
            float totalH = ch * rows + gap * (rows - 1);
            float startX = -totalW * 0.5f + cw * 0.5f;
            float startY = totalH * 0.5f - ch * 0.5f;

            for (int i = 0; i < n; i++)
            {
                int row = i / cols;
                int col = i % cols;
                var rt = (RectTransform)cards[i].transform;
                // centre a final short row
                int inRow = (row == rows - 1) ? (n - row * cols) : cols;
                float rowW = cw * inRow + gap * (inRow - 1);
                float rowStartX = -rowW * 0.5f + cw * 0.5f;

                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(cw, ch);
                rt.anchoredPosition = new Vector2(rowStartX + col * (cw + gap), startY - row * (ch + gap));
            }
        }

        void HandleCardClicked(CardView card)
        {
            AudioManager.I?.PlaySelect();

            if (!multi)
            {
                onPick?.Invoke(card);
                return;
            }

            if (maxSelect == 1)
            {
                bool wasSelected = card.IsSelected;
                foreach (var c in cards) c.SetSelected(false);
                Selection.Clear();
                if (!wasSelected)
                {
                    card.SetSelected(true);
                    Selection.Add(card.Data);
                }
            }
            else
            {
                if (!card.IsSelected && maxSelect > 0 && Selection.Count >= maxSelect)
                {
                    card.Shake();
                    AudioManager.I?.PlaySoftError();
                    return;
                }
                card.SetSelected(!card.IsSelected);
                if (card.IsSelected) Selection.Add(card.Data);
                else Selection.RemoveAll(d => d.id == card.Data.id);
            }

            onToggle?.Invoke(card);
            RefreshConfirmState();
        }

        void RefreshConfirmState()
        {
            if (confirmButton == null) return;
            bool can = allowEmptyConfirm || Selection.Count > 0;
            confirmButton.interactable = can;
        }

        void ConfirmClicked()
        {
            AudioManager.I?.PlayClick();
            var result = new List<CardData>(Selection);
            Hide();
            onConfirm?.Invoke(result);
        }

        void SkipClicked()
        {
            AudioManager.I?.PlayClick();
            Selection.Clear();
            Hide();
            onSkip?.Invoke();
        }

        void ExtraClicked()
        {
            AudioManager.I?.PlayClick();
            Selection.Clear();
            Hide();
            onExtra?.Invoke();
        }

        /// <summary>Programmatic confirm used by board "next step" buttons.</summary>
        public void ConfirmNow()
        {
            ConfirmClicked();
        }
    }
}

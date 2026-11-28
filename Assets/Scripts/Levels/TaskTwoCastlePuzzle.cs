using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoodieLand
{
    [DisallowMultipleComponent]
    public sealed class TaskTwoCastlePuzzle : MonoBehaviour, IBeginDragHandler, IDragHandler,
        IEndDragHandler, IPointerClickHandler
    {
        [Serializable]
        private class Placement
        {
            public Image piece;
            public Image slot;
            public GameObject pieceMarker;
            [NonSerialized] public Vector2 home;
            [NonSerialized] public Color slotColor;
        }

        [SerializeField] private RectTransform castle;
        [SerializeField] private GameObject tray;
        [SerializeField] private Text prompt;
        [SerializeField] private Placement blue;
        [SerializeField] private Placement apricot;
        [SerializeField] private Placement mauve;
        [SerializeField, Min(0f)] private float snapPadding = 16f;

        private RectTransform board;
        private Placement[] placements;
        private Placement current;
        private bool selected;
        private bool returning;
        private int? dragPointer;
        private Vector2 grabOffset;

        private void Awake()
        {
            board = (RectTransform)transform;
            placements = new[] { blue, apricot, mauve };
            foreach (var placement in placements)
            {
                placement.home = placement.piece.rectTransform.anchoredPosition;
                placement.slotColor = placement.slot.color;
            }
        }

        public void ShowTray()
        {
            tray.SetActive(true);
            blue.piece.gameObject.SetActive(true);
            mauve.piece.gameObject.SetActive(true);
        }

        public IEnumerator Play(CharacterView bobo)
        {
            prompt.transform.parent.gameObject.SetActive(true);
            yield return WaitForPlacement(blue, "Your turn! Drag the blue block, or tap it and then tap its spot.");

            prompt.text = "My turn! Watch Bobo place his block.";
            if (bobo != null) bobo.SetExpression("building", restart: true);
            yield return new WaitForSecondsRealtime(0.4f);
            apricot.piece.gameObject.SetActive(true);
            yield return Land(apricot, 0.55f);
            yield return new WaitForSecondsRealtime(0.35f);

            if (bobo != null) bobo.SetExpression("calm");
            yield return WaitForPlacement(mauve, "Your turn! Put the purple block in its spot.");

            prompt.transform.parent.gameObject.SetActive(false);
            tray.SetActive(false);
            if (bobo != null) bobo.SetExpression("happy");
            yield return UIFx.Punch(castle, 0.045f, 0.32f);
        }

        private IEnumerator WaitForPlacement(Placement placement, string instruction)
        {
            prompt.text = instruction;
            current = placement;
            current.piece.raycastTarget = true;
            foreach (var target in placements)
                target.slot.raycastTarget = target.slot.gameObject.activeSelf;

            while (current != null) yield return null;
            yield return Land(placement, 0.18f);
        }

        private IEnumerator Land(Placement placement, float duration)
        {
            var piece = placement.piece.rectTransform;
            var slot = placement.slot.rectTransform;
            piece.SetAsLastSibling();
            var landing = (Vector2)board.InverseTransformPoint(slot.position);
            yield return UIFx.MoveAnchored(piece, landing, duration);

            placement.slot.gameObject.SetActive(false);
            placement.pieceMarker.SetActive(false);
            piece.SetParent(castle, false);
            piece.anchorMin = slot.anchorMin;
            piece.anchorMax = slot.anchorMax;
            piece.anchoredPosition = slot.anchoredPosition;
            AudioManager.I?.PlayPop();
            yield return UIFx.Punch(piece, 0.07f, 0.22f);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (current == null || returning || dragPointer.HasValue ||
                eventData.button != PointerEventData.InputButton.Left ||
                eventData.pointerPressRaycast.gameObject != current.piece.gameObject ||
                !TryGetBoardPoint(eventData.pressPosition, eventData, out var pressPoint)) return;

            SetSelected(false);
            dragPointer = eventData.pointerId;
            var piece = current.piece.rectTransform;
            piece.SetAsLastSibling();
            grabOffset = piece.anchoredPosition - pressPoint;
            MoveDraggedPiece(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragPointer == eventData.pointerId) MoveDraggedPiece(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragPointer != eventData.pointerId) return;

            var hit = eventData.pointerCurrentRaycast.gameObject;
            bool overBoard = hit == null || hit.transform.IsChildOf(transform);
            bool matched = overBoard && MoveDraggedPiece(eventData);
            dragPointer = null;
            if (matched) AcceptPlacement();
            else StartCoroutine(ReturnPiece());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (current == null || returning || dragPointer.HasValue ||
                eventData.button != PointerEventData.InputButton.Left) return;

            var target = eventData.pointerPressRaycast.gameObject;
            if (target != eventData.pointerCurrentRaycast.gameObject) return;

            if (target == current.piece.gameObject)
            {
                SetSelected(!selected);
                AudioManager.I?.PlaySelect();
            }
            else if (selected && target == current.slot.gameObject)
            {
                AcceptPlacement();
            }
            else if (selected)
            {
                StartCoroutine(ReturnPiece(tapped: true));
            }
        }

        private bool MoveDraggedPiece(PointerEventData eventData)
        {
            if (!TryGetBoardPoint(eventData.position, eventData, out var point)) return false;
            var piece = current.piece.rectTransform;
            piece.anchoredPosition = point + grabOffset;

            Placement nearest = null;
            float nearestDistance = float.PositiveInfinity;
            foreach (var placement in placements)
            {
                if (!placement.slot.gameObject.activeSelf) continue;
                var slot = placement.slot.rectTransform;
                var offset = piece.anchoredPosition - (Vector2)board.InverseTransformPoint(slot.position);
                var limit = slot.rect.size * 0.5f + Vector2.one * snapPadding;
                if (Mathf.Abs(offset.x) > limit.x || Mathf.Abs(offset.y) > limit.y) continue;
                if (offset.sqrMagnitude >= nearestDistance) continue;
                nearest = placement;
                nearestDistance = offset.sqrMagnitude;
            }

            bool matched = nearest == current;
            HighlightSlot(matched);
            return matched;
        }

        private bool TryGetBoardPoint(Vector2 screenPoint, PointerEventData data, out Vector2 point)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                board, screenPoint, data.pressEventCamera, out point);
        }

        private void SetSelected(bool value)
        {
            selected = value;
            current.piece.rectTransform.localScale = Vector3.one * (value ? 1.065f : 1f);
            HighlightSlot(value);
        }

        private void HighlightSlot(bool value)
        {
            current.slot.color = value ? new Color(1f, 0.95f, 0.78f, 0.9f) : current.slotColor;
        }

        private void AcceptPlacement()
        {
            SetSelected(false);
            current.piece.raycastTarget = false;
            foreach (var target in placements) target.slot.raycastTarget = false;
            current = null;
        }

        private IEnumerator ReturnPiece(bool tapped = false)
        {
            returning = true;
            SetSelected(false);
            var piece = current.piece.rectTransform;
            if (tapped) yield return UIFx.Shake(piece, 5f, 0.19f);
            else yield return UIFx.MoveAnchored(piece, current.home, 0.22f);
            returning = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus || !dragPointer.HasValue) return;
            dragPointer = null;
            StartCoroutine(ReturnPiece());
        }
    }
}

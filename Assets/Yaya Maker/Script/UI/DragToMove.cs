using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace UIGadget {
	public class DragToMove : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {


		// Api
		public UnityEvent Begin => m_Begin;
		public UnityEvent Drag => m_Drag;
		public UnityEvent End => m_End;

		// Short
		private bool SnapX => m_SnapSize.x.NotAlmostZero();
		private bool SnapY => m_SnapSize.y.NotAlmostZero();

		// Ser
		[SerializeField] Vector2 m_SnapSize = default;
		[SerializeField] bool m_Center = false;
		[SerializeField] UnityEvent m_Begin = null;
		[SerializeField] UnityEvent m_Drag = null;
		[SerializeField] UnityEvent m_End = null;

		// Data
		private Vector2 BeginPos = default;
		private Vector2 Offset = default;


		// MSG
		public void OnBeginDrag (PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left) { return; }
			BeginPos = eventData.position;
			Offset = m_Center ? Vector2.zero : (eventData.position - (transform as RectTransform).anchoredPosition);
			m_Begin.Invoke();
		}


		public void OnDrag (PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left) { return; }
			DragLogic(eventData.position, eventData.pressEventCamera);
			m_Drag.Invoke();
		}


		public void OnEndDrag (PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left) { return; }
			DragLogic(eventData.position, eventData.pressEventCamera);
			m_End.Invoke();
		}


		// LGC
		private void DragLogic (Vector2 pos, Camera camera) {




		}


	}
}

using UnityEngine;

namespace Exdilin.UI
{
    public class MenuBarButton
    {
		public string Title { get; set; }
		private Render OnRenderDelegate;
		private Clean OnCleanDelegate;
		private Init OnInitDelegate;
		private bool useIMGUI = false;

		public MenuBarButton(string title, Render onRender) {
			Title = title;
			OnRenderDelegate = onRender;
			useIMGUI = true;
		}

		public MenuBarButton(string title, Init onInit, Clean onClean) {
			Title = title;
			OnCleanDelegate = onClean;
			OnInitDelegate = onInit;
			useIMGUI = false;
		}

		public void OnClean(Canvas canvas) {
			OnCleanDelegate?.Invoke(canvas);
		}

		public void OnInit(Canvas canvas) {
			OnInitDelegate?.Invoke(canvas);
		}

		public void OnRender() {
			OnRenderDelegate?.Invoke();
		}

		public bool usesIMGUI() {
			return useIMGUI;
		}

		public delegate void Render();
		public delegate void Init(Canvas canvas);
		public delegate void Clean(Canvas canvas);
    }
}

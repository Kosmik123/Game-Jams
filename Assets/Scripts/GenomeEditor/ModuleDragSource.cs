using MicrojamEvolution.Creature;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MicrojamEvolution.GenomeEditor
{
    /// <summary>Sidebar entry that can be dragged onto a block of the creature.</summary>
    public class ModuleDragSource : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private GenomeEditor editor;
        private ModuleType module;

        public void Init(GenomeEditor editor, ModuleType module)
        {
            this.editor = editor;
            this.module = module;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            editor.BeginModuleDrag(module);
        }

        // Has to exist, otherwise the event system does not deliver the drag events.
        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            editor.EndModuleDrag();
        }
    }
}

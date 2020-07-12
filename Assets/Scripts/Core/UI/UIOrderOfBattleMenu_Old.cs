using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Battlehub.UIControls;
using Core.Units;
using Core.Helpers;

namespace Core.UI
{
    /*public class DataItem
    {
        public string Name;

        public DataItem Parent;

        public List<DataItem> Children;

        public DataItem(string name)
        {
            Name = name;
            Children = new List<DataItem>();
        }

        public override string ToString()
        {
            return Name;
        }
    }*/

    public class UIOrderOfBattleMenu_Old : MonoBehaviour
    {
        public VirtualizingTreeView TreeView;

        private List<UnitWrapper> m_dataItems;
        private UnitsRoot unitsRoot;
        private void Start()
        {
            
            /*var unitsRoot = FindObjectOfType<UnitsRoot>();
            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                if (unitsRoot.transform.GetChild(i).GetComponent<Unit>().subTesting)
                {
                    for (int j = 0; j < unitsRoot.transform.childCount; j++)
                    {
                        var u = unitsRoot.transform.GetChild(j).GetComponent<Unit>();
                        var w = (new UnitGroup(false)).GetMyWrapper();
                        if (u.gameObject.name == "Unit (1)" || u.gameObject.name == "Unit (2)")
                        {
                            u.GetMyWrapper().ChangeUnitGroup(w);
                        }
                        UnitGroup.AddSubGroupToGroup(w, unitsRoot.transform.GetChild(i).GetComponent<Unit>().GetMyWrapper().unitsGroupWrapper);
                    }
                }
            }*/
            
            TreeView.ItemDataBinding += OnItemDataBinding;
            TreeView.SelectionChanged += OnSelectionChanged;
            TreeView.ItemsRemoved += OnItemsRemoved;
            TreeView.ItemExpanding += OnItemExpanding;
            TreeView.ItemBeginDrag += OnItemBeginDrag;

            TreeView.ItemDrop += OnItemDrop;
            TreeView.ItemBeginDrop += OnItemBeginDrop;
            TreeView.ItemEndDrag += OnItemEndDrag;

            m_dataItems = new List<UnitWrapper>();
            /*for (int i = 0; i < 100; ++i)
            {
                UnitGroupWrapper dataItem = new UnitGroupWrapper("DataItem " + i);
                m_dataItems.Add(dataItem);
            }*/
            
            unitsRoot = FindObjectOfType<UnitsRoot>();
            for (int i = 0; i < unitsRoot.transform.childCount; i++)
            {
                var u = unitsRoot.transform.GetChild(i).GetComponent<UnitPiece>();
                //Add(u.GetMyWrapper().unitsGroupWrapper, u.GetMyWrapper().unitsGroupWrapper.GetParentNode());
                m_dataItems.Add(u.GetRefWrapper().unitWrapper);
                //u.GetMyWrapper().SubscribeOnClearance(() => m_dataItems.R);
            }
            TreeView.Items = m_dataItems;

            if(m_buttons != null)
            {
                m_buttons.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            TreeView.ItemDataBinding -= OnItemDataBinding;
            TreeView.SelectionChanged -= OnSelectionChanged;
            TreeView.ItemsRemoved -= OnItemsRemoved;
            TreeView.ItemExpanding -= OnItemExpanding;
            TreeView.ItemBeginDrag -= OnItemBeginDrag;
            TreeView.ItemBeginDrop -= OnItemBeginDrop;
            TreeView.ItemDrop -= OnItemDrop;
            TreeView.ItemEndDrag -= OnItemEndDrag;
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            //get parent data item (game object in our case)
            UnitWrapper dataItem = (UnitWrapper)e.Item;
            if (dataItem.GetChildNodes().Count > 0)
            {
                //Populate children collection
                e.Children = dataItem.GetChildNodes();
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            #if UNITY_EDITOR
            //Do something on selection changed (just syncronized with editor's hierarchy for demo purposes)
           // UnityEditor.Selection.objects = e.NewItems.OfType<GameObject>().ToArray();
            #endif

            if(m_buttons != null)
            {
                m_buttons.SetActive(TreeView.SelectedItem != null);
            }
        }

        private void OnItemsRemoved(object sender, ItemsRemovedArgs e)
        {
            //Destroy removed dataitems
            for (int i = 0; i < e.Items.Length; ++i)
            {
                UnitWrapper dataItem = (UnitWrapper)e.Items[i];
                if(dataItem.GetParentNode() != null)
                {
                    dataItem.GetParentNode().RemoveChild(dataItem);
                }
                m_dataItems.Remove(dataItem);
            }
        }

        /// <summary>
        /// This method called for each data item during databinding operation
        /// You have to bind data item properties to ui elements in order to display them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            UnitWrapper dataItem = e.Item as UnitWrapper;
            if (dataItem != null)
            {   
                //We display dataItem.name using UI.Text 
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = Unit.GetUnitPieceWrappersInUnit(dataItem)[0].GetWrappedReference().gameObject.name;

                //Load icon from resources
                Image icon = e.ItemPresenter.GetComponentsInChildren<Image>()[4];
                icon.sprite = Resources.Load<Sprite>("IconNew");

                //And specify whether data item has children (to display expander arrow if needed)

                e.HasChildren = dataItem.GetChildNodes().Count > 0;
                
            }
        }

        private void OnItemBeginDrop(object sender, ItemDropCancelArgs e)
        {

        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            //Could be used to change cursor
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {
        }


        private List<UnitWrapper> ChildrenOf(UnitWrapper parent)
        {
            if(parent == null)
            {
                return m_dataItems;
            }
            return parent.GetChildNodes();
        }

        private void OnItemDrop(object sender, ItemDropArgs args)
        {
            if(args.DropTarget == null)
            {
                return;
            }

            TreeView.ItemDropStdHandler<UnitWrapper>(args,
                (item) => item.GetParentNode(),
                (item, parent) => item.ChangeParentTo(parent),
                (item, parent) => ChildrenOf(parent).IndexOf(item),
                (item, parent) => ChildrenOf(parent).Remove(item),
                (item, parent, i) => ChildrenOf(parent).Insert(i, item));
        }

        [SerializeField]
        private GameObject m_buttons = null;
        private int m_counter = 0;

        public void ScrollIntoView()
        {
            TreeView.ScrollIntoView(TreeView.SelectedItem);
        }

        public void Add(UnitWrapper ugw, UnitWrapper parent)
        {   
            TreeView.AddChild(parent, ugw);
            //TreeView.Expand(parent);

            m_counter++;
        }

        public void Remove()
        {
            foreach (UnitWrapper selectedItem in TreeView.SelectedItems.OfType<object>().ToArray())
            {
                TreeView.RemoveChild(selectedItem.GetParentNode(), selectedItem);                
            }
        }

        public void Collapse()
        {
            foreach (UnitWrapper selectedItem in TreeView.SelectedItems)
            {
                TreeView.Collapse(selectedItem);
            }
        }

        public void Expand()
        {
            foreach (UnitWrapper selectedItem in TreeView.SelectedItems)
            {
                TreeView.ExpandAll(selectedItem, item => item.GetParentNode(), item => item.GetChildNodes());
            }
        }

        /*
        public void Add()
        {
            foreach (UnitGroupWrapper parent in TreeView.SelectedItems)
            {
                UnitGroupWrapper item = null;//new DataItem("New Item");
                parent.AddChild(item);
                item.ChangeParentTo(parent);

                TreeView.AddChild(parent, item);
                TreeView.Expand(parent);

                UnitGroupWrapper subItem = null;//new DataItem("New Sub Item");
                item.AddChild(subItem);
                subItem.ChangeParentTo(item);

                TreeView.AddChild(item, subItem);
                TreeView.Expand(item);

                m_counter++;
            }
        }

        public void Remove()
        {
            foreach (UnitGroupWrapper selectedItem in TreeView.SelectedItems.OfType<object>().ToArray())
            {
                TreeView.RemoveChild(selectedItem.GetParentNode(), selectedItem);                
            }
        }

        public void Collapse()
        {
            foreach (UnitGroupWrapper selectedItem in TreeView.SelectedItems)
            {
                TreeView.Collapse(selectedItem);
            }
        }

        public void Expand()
        {

            foreach (UnitGroupWrapper selectedItem in TreeView.SelectedItems)
            {
                
                TreeView.ExpandAll(selectedItem, item => item.GetParentNode(), item => item.GetChildNodes());
            }
        }
        */

    }
}

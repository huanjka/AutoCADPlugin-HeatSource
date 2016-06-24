using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace HeatSource.View.SolutionAtrrBean
{
    public class CustomAttrCollectionDescriptor : PropertyDescriptor
    {
        private CustomAttrCollection collection = null;
        private int index = -1;

        public CustomAttrCollectionDescriptor(CustomAttrCollection coll, int idx) :
            base("#" + idx.ToString(), null)
        {
            this.collection = coll;
            this.index = idx;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                var e = new ExpandableObjectAttribute();
                var c = new CategoryAttribute(Constants.CATEGORY_DETAIL);
                var o = new PropertyOrderAttribute(index);
                Attribute[] b = new Attribute[3];
                b[0] = e;
                b[1] = c;
                b[2] = o;
                var a = new AttributeCollection(b);
                return a;
            }
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get
            {
                return this.collection.GetType();
            }
        }

        public override string DisplayName
        {
            get
            {
                AttrInfo attr = this.collection[index];
                return attr.name;
            }
        }

        public override string Description
        {
            get
            {
                return "";
            }
        }

        public override object GetValue(object component)
        {
            AttrInfo attr = this.collection[index];
            return attr;
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override string Name
        {
            get { return "#" + index.ToString(); }
        }

        public override Type PropertyType
        {
            get { return this.collection[index].GetType(); }
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        public override void SetValue(object component, object value)
        {
            // this.collection[index] = value;
        }
    }
}

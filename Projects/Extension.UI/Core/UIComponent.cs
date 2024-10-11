using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.UI
{
    public class UIComponent
    {

        private bool _initialized = false;

        private bool _visible;

        public bool Visible
        {
            get{
                return _visible;
            }
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    VisibleChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<EventArgs> VisibleChanged;


        public virtual void Initialize()
        {
            if(!_initialized)
            {
                _initialized = true;
            }
        }

        public virtual void Update() 
        {
        
        }

        public virtual void Draw()
        {

        }
    }
}

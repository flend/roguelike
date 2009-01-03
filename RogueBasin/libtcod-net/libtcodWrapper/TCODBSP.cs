using System;
using System.Runtime.InteropServices;

namespace libtcodWrapper
{
    #pragma warning disable 1591  //Disable warning about lack of xml comments

    public delegate bool TCODBSPTraversalDelegate(TCODBSP bsp);

    unsafe public class TCODBSP : IDisposable
    {
        private TCODBSPData * m_data;
        private TCODBSPTraversalDelegate m_delegate;
        private TCODBSPTraversalDelegatePrivate m_privateDelegate;

        #region Properties
        public int x
        {
            get
            {
                return m_data->x;
            }
			set
			{
				m_data->x = value;
			}
        }
        public int y
        {
            get
            {
                return m_data->y;
            }
			set
			{
				m_data->y = value;
			}
        }
        public int w
        {
            get
            {
                return m_data->w;
            }
			set
			{
				m_data->w = value;
			}
        }
        public int h
        {
            get
            {
                return m_data->h;
            }
			set
			{
				m_data->h = value;
			}
        }
        public int position
        {
            get
            {
                return m_data->position;
            }
        }
        public bool horizontal
        {
            get
            {
                return m_data->horizontal;
            }
        }
        public byte level
        {
            get
            {
                return m_data->level;
            }
        }
        #endregion

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool TCODBSPTraversalDelegatePrivate(IntPtr bsp, IntPtr nullPtr);

        public TCODBSP()
        {
            m_data = (TCODBSPData *)TCOD_bsp_new();
            m_delegate = null;
            m_privateDelegate = new TCODBSPTraversalDelegatePrivate(this.TCODBSPTraversalDel);
        }

        public TCODBSP(int x, int y, int w, int h)
        {
            m_data = (TCODBSPData *)TCOD_bsp_new_with_size(x, y, w, h);
            m_delegate = null;
            m_privateDelegate = new TCODBSPTraversalDelegatePrivate(this.TCODBSPTraversalDel);
        }

        private TCODBSP(TCODBSPData* data)
        {
            m_data = data;
            m_delegate = null;
            m_privateDelegate = new TCODBSPTraversalDelegatePrivate(this.TCODBSPTraversalDel);
        }

        public void Dispose()
        {
            if (m_data != null)
                TCOD_bsp_delete(new IntPtr(m_data));
        }

        public static bool operator ==(TCODBSP lhs, TCODBSP rhs)
        {
            return ((*lhs.m_data) == *(rhs.m_data));
        }

        public static bool operator !=(TCODBSP lhs, TCODBSP rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(Object obj)
        {
            // Return true if the fields match:
            return base.Equals((TCODBSPData)obj) && (this == (TCODBSP)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        bool TCODBSPTraversalDel(IntPtr bsp, IntPtr nullPtr)
        {
            if (m_delegate != null)
            {
                TCODBSPData* p = (TCODBSPData*)bsp;
                if (p != null)
                    return m_delegate(new TCODBSP(p));
                else
                    return m_delegate(null);
            }
            return false;
        }

        public TCODBSP GetLeft()
        {
            TCODBSPData *p = (TCODBSPData*)TCOD_bsp_left(new IntPtr(m_data));
            if (p != null)
                return new TCODBSP(p);
            return null;
        }

        public TCODBSP GetRight()
        {
            TCODBSPData* p = (TCODBSPData*)TCOD_bsp_right(new IntPtr(m_data));
            if (p != null)
                return new TCODBSP(p);
            return null;
        }

        public TCODBSP GetFather()
        {
            TCODBSPData* p = (TCODBSPData*)TCOD_bsp_father(new IntPtr(m_data));
            if (p != null)
                return new TCODBSP(p);
            return null;
        }

        public TCODBSP FindNode(int x, int y)
        {
            TCODBSPData* p = (TCODBSPData*)TCOD_bsp_find_node(new IntPtr(m_data), x, y);
            if (p != null)
                return new TCODBSP(p);
            return null;
        }

        public void SplitOnce(bool horizontal, int position)
        {
            TCOD_bsp_split_once(new IntPtr(m_data), horizontal, position);
        }

        public bool IsLeaf()
        {
            return TCOD_bsp_is_leaf(new IntPtr(m_data));
        }

        public void Resize(int x, int y, int w, int h)
        {
            TCOD_bsp_resize(new IntPtr(m_data), x, y, w, h);
        }

        public void RemoveSons()
        {
            TCOD_bsp_remove_sons(new IntPtr(m_data));
        }

        public void SplitRecursive(TCODRandom randomizer, int nb, int minHSize, int minVSize, float maxHRatio, float maxVRatio)
        {
            TCOD_bsp_split_recursive(new IntPtr(m_data), randomizer.m_instance, nb, minHSize, minVSize, maxHRatio, maxVRatio);
        }

        public bool IsMapCellInsideNode(int x, int y)
        {
            return TCOD_bsp_contains(new IntPtr(m_data), x, y);
        }

        public bool TraversePreOrder(TCODBSPTraversalDelegate listner)
        {
            m_delegate = listner;
            return TCOD_bsp_traverse_pre_order(new IntPtr(m_data), m_privateDelegate, IntPtr.Zero);
        }

        public bool TraverseInOrder(TCODBSPTraversalDelegate listner)
        {
            m_delegate = listner;
            return TCOD_bsp_traverse_in_order(new IntPtr(m_data), m_privateDelegate, IntPtr.Zero);
        }

        public bool TraversePostOrder(TCODBSPTraversalDelegate listner)
        {
            m_delegate = listner;
            return TCOD_bsp_traverse_in_order(new IntPtr(m_data), m_privateDelegate, IntPtr.Zero);
        }

        public bool TraverseLevelOrder(TCODBSPTraversalDelegate listner)
        {
            m_delegate = listner;
            return TCOD_bsp_traverse_level_order(new IntPtr(m_data), m_privateDelegate, IntPtr.Zero);
        }

        public bool TraverseInvertedOrder(TCODBSPTraversalDelegate listner)
        {
            m_delegate = listner;
            return TCOD_bsp_traverse_inverted_level_order(new IntPtr(m_data), m_privateDelegate, IntPtr.Zero);
        }

        #region DllImport

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_bsp_new();

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_bsp_new_with_size(int x, int y, int w, int h);
        
        [DllImport(DLLName.name)]
        private extern static void TCOD_bsp_delete(IntPtr node);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_bsp_left(IntPtr node);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_bsp_right(IntPtr node);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_bsp_find_node(IntPtr node, int x, int y);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_bsp_father(IntPtr node);

        [DllImport(DLLName.name)]
        private extern static void TCOD_bsp_split_once(IntPtr node, bool horizontal, int position);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_bsp_is_leaf(IntPtr node);
        
        [DllImport(DLLName.name)]
        private extern static void TCOD_bsp_resize(IntPtr node, int x, int y, int w, int h);

        [DllImport(DLLName.name)]
        private extern static void TCOD_bsp_remove_sons(IntPtr node);

        [DllImport(DLLName.name)]
        private extern static void TCOD_bsp_split_recursive(IntPtr node, IntPtr randomizer, int nb, int minHSize, int minVSize, float maxHRatio, float maxVRatio);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_bsp_traverse_pre_order(IntPtr node, TCODBSPTraversalDelegatePrivate listener, IntPtr userData);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_bsp_traverse_in_order(IntPtr node, TCODBSPTraversalDelegatePrivate listener, IntPtr userData);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_bsp_traverse_post_order(IntPtr node, TCODBSPTraversalDelegatePrivate listener, IntPtr userData);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_bsp_traverse_level_order(IntPtr node, TCODBSPTraversalDelegatePrivate listener, IntPtr userData);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_bsp_traverse_inverted_level_order(IntPtr node, TCODBSPTraversalDelegatePrivate listener, IntPtr userData);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_bsp_contains(IntPtr node, int x, int y);

        #endregion

        [StructLayout(LayoutKind.Sequential)]
        private struct TCODBSPData
        {
            public IntPtr ptr1;
            public IntPtr ptr2;
            public IntPtr ptr3;
            public int x;
            public int y;
            public int w;
            public int h;
            public int position;
            public byte level;
            public bool horizontal;

            public static bool operator ==(TCODBSPData lhs, TCODBSPData rhs)
            {
                return (lhs.x == rhs.x && lhs.y == rhs.y && lhs.w == rhs.w &&
                    lhs.h == rhs.h && lhs.level == rhs.level);
            }

            public static bool operator !=(TCODBSPData lhs, TCODBSPData rhs)
            {
                return !(lhs == rhs);
            }

            public override bool Equals(Object obj)
            {
                // Return true if the fields match:
                return base.Equals((TCODBSPData)obj) && (this == (TCODBSPData)obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }

    #pragma warning restore 1591  //Disable warning about lack of xml comments
}

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace libtcodWrapper
{
    /// <summary>
    /// Types of values parsed from config file
    /// </summary>
    public enum TCODValueType
    {
        #pragma warning disable 1591  //Disable warning about lack of xml comments
        TCOD_TYPE_NONE,
        TCOD_TYPE_BOOL,
        TCOD_TYPE_CHAR,
        TCOD_TYPE_INT,
        TCOD_TYPE_FLOAT,
        TCOD_TYPE_STRING,
        TCOD_TYPE_COLOR,
        TCOD_TYPE_DICE,
        TCOD_TYPE_VALUELIST00,
        TCOD_TYPE_VALUELIST01,
        TCOD_TYPE_VALUELIST02,
        TCOD_TYPE_VALUELIST03,
        TCOD_TYPE_VALUELIST04,
        TCOD_TYPE_VALUELIST05,
        TCOD_TYPE_VALUELIST06,
        TCOD_TYPE_VALUELIST07,
        TCOD_TYPE_VALUELIST08,
        TCOD_TYPE_VALUELIST09,
        TCOD_TYPE_VALUELIST10,
        TCOD_TYPE_VALUELIST11,
        TCOD_TYPE_VALUELIST12,
        TCOD_TYPE_VALUELIST13,
        TCOD_TYPE_VALUELIST14,
        TCOD_TYPE_VALUELIST15,
        TCOD_TYPE_CUSTOM00,
        TCOD_TYPE_CUSTOM01,
        TCOD_TYPE_CUSTOM02,
        TCOD_TYPE_CUSTOM03,
        TCOD_TYPE_CUSTOM04,
        TCOD_TYPE_CUSTOM05,
        TCOD_TYPE_CUSTOM06,
        TCOD_TYPE_CUSTOM07,
        TCOD_TYPE_CUSTOM08,
        TCOD_TYPE_CUSTOM09,
        TCOD_TYPE_CUSTOM10,
        TCOD_TYPE_CUSTOM11,
        TCOD_TYPE_CUSTOM12,
        TCOD_TYPE_CUSTOM13,
        TCOD_TYPE_CUSTOM14,
        TCOD_TYPE_CUSTOM15,
        TCOD_TYPE_LIST=1024
        #pragma warning restore 1591  //Disable warning about lack of xml comments
    }

    /// <summary>
    /// "Union" that holds value obtained from config file
    /// </summary>
    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
    unsafe public struct TCODValue
    {
        #pragma warning disable 1591  //Disable warning about lack of xml comments
        [FieldOffset(0)]
        public bool b;
          
        [FieldOffset(0)]
        public byte c;
        
        [FieldOffset(0)]
        public int i; 
        
        [FieldOffset(0)]
        public float f;

        [FieldOffset(0)]
        public fixed char s[512];        

        [FieldOffset(0)]
        public Color col;
        
        [FieldOffset(0)]
        public TCODDice dice;

        [FieldOffset(0)]
        public IntPtr list;

        [FieldOffset(0)]
        public IntPtr custom;
        #pragma warning restore 1591  //Disable warning about lack of xml comments

        /// <summary>
        /// Marshal a string from the fixed field 's'
        /// </summary>
        /// <returns>String</returns>
        public string GetStringFromFieldS()
        {
            return Marshal.PtrToStringAnsi(custom);
        }
    }

    /// <summary>
    /// Hold dice-type ranges: [multiplier x] nb_dices d nb_faces [(+|-) addsub]
    /// </summary>
    [StructLayout(LayoutKind.Sequential) ]
    public struct TCODDice
    {
        /// <summary>
        /// Number of Dices
        /// </summary>
        public int nb_dices;

        /// <summary>
        /// Number of faces per side
        /// </summary>
        public int nb_faces;

        /// <summary>
        /// Multiplier attached to dice roll
        /// </summary>
        public float multiplier;

        /// <summary>
        /// Constant to add/subtract to dice roll
        /// </summary>
        public float addsub;

        /// <summary>
        /// Create a new TCODDice
        /// </summary>
        /// <param name="dices">Number of dice</param>
        /// <param name="faces">Number of face per dice</param>
        /// <param name="mult">Multiplier to roll</param>
        /// <param name="constant">Constant to add/subtract</param>
        public TCODDice(int dices, int faces, int mult, int constant)
        {
            nb_dices = dices;
            nb_faces = faces;
            multiplier = mult;
            addsub = constant;
        }

        /// <summary>
        /// Create a new TCODDice
        /// </summary>
        /// <param name="dices">Number of dice</param>
        /// <param name="faces">Number of face per dice</param>
        public TCODDice(int dices, int faces)
        {
            nb_dices = dices;
            nb_faces = faces;
            multiplier = 1;
            addsub = 0;
        }

        /// <summary>
        /// Create Copy of TCODDIce
        /// </summary>
        /// <param name="d">Dice to copy</param>
        public TCODDice(TCODDice d)
        {
            nb_dices = d.nb_dices;
            nb_faces = d.nb_faces;
            multiplier = d.multiplier;
            addsub = d.addsub;
        }
        
        /// <summary>
        /// Compare two TCODDice
        /// </summary>
        /// <param name="obj">Other object</param>
        /// <returns>Are Equal?</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            TCODDice rhs = (TCODDice)obj;
            return (nb_dices == rhs.nb_dices) && (nb_faces == rhs.nb_faces) && (multiplier == rhs.multiplier) && (addsub == rhs.addsub);
        }

        /// <summary>
        /// Calculate Hash Value of a TCODDice
        /// </summary>
        /// <returns>Hash Value</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Determine if two TCODDice are equal.
        /// </summary>
        /// <param name="lhs">Left Hand Side</param>
        /// <param name="rhs">Right Hand Side</param>
        /// <returns>Are Equal?</returns>
        public static bool operator ==(TCODDice lhs, TCODDice rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Determine if two TCODDice are not equal.
        /// </summary>
        /// <param name="lhs">Left Hand Side</param>
        /// <param name="rhs">Right Hand Side</param>
        /// <returns>Are Not Equal?</returns>
        public static bool operator !=(TCODDice lhs, TCODDice rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
    
    /// <summary>
    /// Callback from parser when new structure is found
    /// </summary>
    /// <param name="str">New Structure</param>
    /// <param name="name">Structure Name</param>
    /// <returns>Return true if parsing is successful. False causes abort()</returns>
    public delegate bool NewStructureCallback(TCODParserStructure str, string name);

    /// <summary>
    /// Callback from parser when new flag is found
    /// </summary>
    /// <param name="name">Name of flag</param>
    /// <returns>Return true if parsing is successful. False causes abort()</returns>
    public delegate bool NewFlagCallback(string name);

    /// <summary>
    /// Callback from parser when new property is found
    /// </summary>
    /// <param name="name">Name of new property</param>
    /// <param name="type">Type of new property</param>
    /// <param name="v">Value of new property</param>
    /// <returns>Return true if parsing is successful. False causes abort()</returns>
    public delegate bool NewPropertyCallback(string name, TCODValueType type, TCODValue v);

    /// <summary>
    /// Callback from parser when end of structure is found
    /// </summary>
    /// <param name="str">Structure which end is found</param>
    /// <param name="name">Name of structure which end is found</param>
    /// <returns>Return true if parsing is successful. False causes abort()</returns>
    public delegate bool EndStructureCallback(TCODParserStructure str, string name);

    /// <summary>
    /// Callback from parser when parsing error occurs
    /// </summary>
    /// <param name="msg">Error message from parser</param>
    public delegate void ErrorCallback(string msg);
    

    /// <summary>
    /// Holds onto callbacks the parser uses to communicate.
    /// </summary>
    public class TCODParserCallbackStruct
    {
        private NewStructureCallback ns;
        private NewFlagCallback nf;
        private NewPropertyCallback np;
        private EndStructureCallback es;
        private ErrorCallback er;
        internal TCODParserNativeCallback nativeCallback;
        
        /// <summary>
        /// Create CallbackStruct which passes callbacks to parser
        /// </summary>
        /// <param name="newStruct">Callback when new structure is found</param>
        /// <param name="newFlag">Callback when new flag is found</param>
        /// <param name="newProp">Callback when new property is found</param>
        /// <param name="endStruct">Callback when new of structure is found</param>
        /// <param name="error">Callback when parser comes across error</param>
        public TCODParserCallbackStruct(NewStructureCallback newStruct, NewFlagCallback newFlag, NewPropertyCallback newProp,
                                 EndStructureCallback endStruct, ErrorCallback error)
        {
            ns = newStruct;
            nf = newFlag;
            np = newProp;
            es = endStruct;
            er = error;
            nativeCallback = new TCODParserNativeCallback();
            nativeCallback.new_structure = new new_struct_delegate(this.NativeNewStructCallback); 
            nativeCallback.new_flag = new new_flag_delegate(this.NativeNewFlagCallback);
            nativeCallback.new_property = new new_property_delegate(this.NativePropertyCallback);
            nativeCallback.end_structure = new end_struct_delegate(this.NativeEndStructCallback);
            nativeCallback.error = new error_delegate(this.NativeErrorCallback);
        }

        private static string GetStringIfValid(StringBuilder name)
        {
            return (name != null ? name.ToString() : null);
        }
        
        private bool NativeNewStructCallback(IntPtr str, StringBuilder name)
        {
            TCODParserStructure cur = new TCODParserStructure(str);
            return ns(cur, GetStringIfValid(name));
        }

        private bool NativeNewFlagCallback(StringBuilder name)
        {
            return nf(GetStringIfValid(name));
        }

        private bool NativePropertyCallback(StringBuilder name, TCODValueType type, TCODValue v)
        {
            return np(GetStringIfValid(name), type, v);
        }

        private bool NativeEndStructCallback(IntPtr str, StringBuilder name)
        {
            TCODParserStructure cur = new TCODParserStructure(str);
            return es(cur, GetStringIfValid(name));
        }

        private void NativeErrorCallback(StringBuilder msg)
        {
            er(GetStringIfValid(msg));
        }
        
        /// <summary>
        /// If called in delegate handing parser events, will return string to be outputted along with position and abort the parsing.
        /// </summary>
        /// <param name="error">String explaining error</param>
        public void ReturnErrorToParser(string error)
        {
            TCOD_parser_error(new StringBuilder(error));
        }

        [StructLayout(LayoutKind.Sequential) ]
        internal struct TCODParserNativeCallback
        {
            internal new_struct_delegate new_structure;
            internal new_flag_delegate new_flag;
            internal new_property_delegate new_property;
            internal end_struct_delegate end_structure;
            internal error_delegate error;
        };
        
        [DllImport(DLLName.name)]
        private extern static void TCOD_parser_error(StringBuilder msg);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool new_struct_delegate(IntPtr str, StringBuilder name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool new_flag_delegate(StringBuilder name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool new_property_delegate(StringBuilder name, TCODValueType type, TCODValue v);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool end_struct_delegate(IntPtr str, StringBuilder name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]    
        internal delegate void error_delegate(StringBuilder msg);
    }

    /// <summary>
    /// Parses configuration file
    /// </summary>
    public class TCODFileParser : IDisposable
    {
        internal IntPtr m_fileParser;

        /// <summary>
        /// Create new parser
        /// </summary>
        public TCODFileParser()
        {
            m_fileParser = TCOD_parser_new();
        }

        /// <summary>
        /// Destory unmanaged parser resource
        /// </summary>
        public void Dispose()
        {
            TCOD_parser_delete(m_fileParser);
        }

        /// <summary>
        /// Run the parser with custom callbacks
        /// </summary>
        /// <param name="filename">Filename of configuration file</param>
        /// <param name="listener">Callbacks from parser</param>
        public void Run(string filename, ref TCODParserCallbackStruct listener)
        {
            TCOD_parser_run(m_fileParser, new StringBuilder(filename), ref listener.nativeCallback);
        }

        /// <summary>
        /// Run the parser with the default parser listener
        /// </summary>
        /// <param name="filename">Filename of configuration file</param>
        public void Run(string filename)
        {
            TCOD_parser_run(m_fileParser, new StringBuilder(filename), IntPtr.Zero);
        }

        /// <summary>
        /// Register a new structure with the parser
        /// </summary>
        /// <param name="name">Structure Name</param>
        /// <returns></returns>
        public TCODParserStructure RegisterNewStructure(string name)
        {
            return new TCODParserStructure(TCOD_parser_new_struct(m_fileParser, new StringBuilder(name)));
        }

        /// <summary>
        /// Get a boolean property from the default parser listener
        /// </summary>
        /// <param name="name">Property Name</param>
        /// <remarks>Use only if you use the default parser listener</remarks>
        /// <returns>Boolean Value of Property</returns>
        public bool GetBoolProperty(string name)
        {
            return TCOD_parser_get_bool_property(m_fileParser, new StringBuilder(name));
        }

        /// <summary>
        /// Get a integer property from the default parser listener
        /// </summary>
        /// <param name="name">Property Name</param>
        /// /// <remarks>Use only if you use the default parser listener</remarks>
        /// <returns>Int Value of Property</returns>
        public int GetIntProperty(string name)
        {
            return TCOD_parser_get_int_property(m_fileParser, new StringBuilder(name));
        }

        /// <summary>
        /// Get a float property from the default parser listener
        /// </summary>
        /// <param name="name">Property Name</param>
        /// /// <remarks>Use only if you use the default parser listener</remarks>
        /// <returns>Float Value of Property</returns>
        public float GetFloatProperty(string name)
        {
            return TCOD_parser_get_float_property(m_fileParser, new StringBuilder(name));
        }

        /// <summary>
        /// Get a string property from the default parser listener
        /// </summary>
        /// <param name="name">Property Name</param>
        /// /// <remarks>Use only if you use the default parser listener</remarks>
        /// <returns>String Value of Property</returns>
        public string GetStringProperty(string name)
        {
            return TCOD_parser_get_string_property_helper(m_fileParser, new StringBuilder(name));
        }

        /// <summary>
        /// Get a color property from the default parser listener
        /// </summary>
        /// <param name="name">Property Name</param>
        /// /// <remarks>Use only if you use the default parser listener</remarks>
        /// <returns>Color Value of Property</returns>
        public Color GetColorProperty(string name)
        {
            return TCOD_parser_get_color_property(m_fileParser, new StringBuilder(name));
        }

        /// <summary>
        /// Get the dice property from the default parser listener
        /// </summary>
        /// <param name="name">Property Name</param>
        /// /// <remarks>Use only if you use the default parser listener</remarks>
        /// <returns>Dice Value of Property</returns>
        public TCODDice GetDiceProperty(string name)
        {
            return TCOD_parser_get_dice_property(m_fileParser, new StringBuilder(name));
        }

        #region DllImport
        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_parser_new();

        [DllImport(DLLName.name)]
        private extern static void TCOD_parser_run(IntPtr parser, StringBuilder filename, ref TCODParserCallbackStruct.TCODParserNativeCallback listener);

        [DllImport(DLLName.name)]
        private extern static void TCOD_parser_run(IntPtr parser, StringBuilder filename, IntPtr nullListener);

        [DllImport(DLLName.name)]
        private extern static void TCOD_parser_delete(IntPtr parser);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_parser_new_struct(IntPtr parser, StringBuilder name);

        [DllImport(DLLName.name)]
        private extern static bool TCOD_parser_get_bool_property(IntPtr parser, StringBuilder name);
        
        [DllImport(DLLName.name)]
        private extern static int TCOD_parser_get_int_property(IntPtr parser, StringBuilder name);
        
        [DllImport(DLLName.name)]
        private extern static float TCOD_parser_get_float_property(IntPtr parser, StringBuilder name);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_parser_get_string_property(IntPtr parser, StringBuilder name);
        
        private static string TCOD_parser_get_string_property_helper(IntPtr parser, StringBuilder name)
        {
            return Marshal.PtrToStringAnsi(TCOD_parser_get_string_property(parser, name));
        }

        [DllImport(DLLName.name)]
        private extern static Color TCOD_parser_get_color_property(IntPtr parser, StringBuilder name);

        [DllImport(DLLName.name)]
        private extern static TCODDice TCOD_parser_get_dice_property(IntPtr parser, StringBuilder name);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_parser_get_list_property(IntPtr parser, StringBuilder name, TCODValueType type);

        #endregion
    }

    /// <summary>
    /// Created by RegisterNewStructure and represents one valid "structure in the config file"
    /// </summary>
    public class TCODParserStructure
    {
        internal TCODParserStructure(IntPtr p)
        {
            m_parserStructure = p;
        }

        internal IntPtr m_parserStructure;

        /// <summary>
        /// Add flag to structure
        /// </summary>
        /// <param name="name">Flag Name</param>
        public void AddFlag(string name)
        {
            TCOD_struct_add_flag(m_parserStructure, new StringBuilder(name));
        }

        /// <summary>
        /// Add new property to structure
        /// </summary>
        /// <param name="name">Name of Property</param>
        /// <param name="type">Property Type</param>
        /// <param name="mandatory">Is Mandatory?</param>
        public void AddProperty(string name, TCODValueType type, bool mandatory)
        {
            TCOD_struct_add_property(m_parserStructure, new StringBuilder(name), type, mandatory);
        }

        /// <summary>
        /// Add new "value list", set of possible string values
        /// </summary>
        /// <param name="name">Name of List</param>
        /// <param name="list">Possible Values</param>
        /// <param name="mandatory">Is Mandatory?</param>
        public void AddValueList(string name, string[] list, bool mandatory)
        {
            TCOD_struct_add_value_list_sized(m_parserStructure, new StringBuilder(name), list, list.Length, mandatory);
        }

        /// <summary>
        /// Add substructure to structure
        /// </summary>
        /// <param name="substructure">New Substructure to add</param>
        public void AddSubStructure(TCODParserStructure substructure)
        {
            TCOD_struct_add_structure(m_parserStructure, substructure.m_parserStructure);
        }

        /// <summary>
        /// Get Name of Structure
        /// </summary>
        /// <returns>Name</returns>
        public string GetName()
        {
            return TCOD_struct_get_name_helper(m_parserStructure);
        }

        /// <summary>
        /// Returns if structure is a mandatory value
        /// </summary>
        /// <param name="name">Name of Structure</param>
        /// <returns>Is Mandatory?</returns>
        public bool IsMandatory(string name)
        {
            return TCOD_struct_is_mandatory(m_parserStructure, new StringBuilder(name));
        }

        /// <summary>
        /// Get Value's type of structure
        /// </summary>
        /// <param name="name">Property's Name</param>
        /// <returns>Type</returns>
        public TCODValueType GetType(string name)
        {
            return TCOD_struct_get_type(m_parserStructure, new StringBuilder(name));
        }

        #region DllImport
        [DllImport(DLLName.name)]
        private extern static void TCOD_struct_add_flag(IntPtr str, StringBuilder name);

        [DllImport(DLLName.name)]
        private extern static void TCOD_struct_add_property(IntPtr def, StringBuilder name, TCODValueType type, bool mandatory);

        [DllImport(DLLName.name)]
        private extern static void TCOD_struct_add_list_property(IntPtr def, StringBuilder name, TCODValueType type, bool mandatory);

        [DllImport(DLLName.name)]
        private extern static void TCOD_struct_add_value_list_sized(IntPtr def, StringBuilder name, [In, Out] String[] value_list, int size, bool mandatory);

        [DllImport(DLLName.name)]
        private extern static void TCOD_struct_add_structure(IntPtr str, IntPtr sub_structure);

        [DllImport(DLLName.name)]
        private extern static IntPtr TCOD_struct_get_name(IntPtr str);
        
        private static string TCOD_struct_get_name_helper(IntPtr str)
        {
            return Marshal.PtrToStringAnsi(TCOD_struct_get_name(str));
        }

        [DllImport(DLLName.name)]
        private extern static bool TCOD_struct_is_mandatory(IntPtr str, StringBuilder name);

        [DllImport(DLLName.name)]
        private extern static TCODValueType TCOD_struct_get_type(IntPtr str, StringBuilder name);
        #endregion
    }
}

using UnityEngine;
using System.Collections;

namespace AtRng.MobileTTA {
    public class GuidMB : MonoBehaviour {
/*
        [SerializeField] byte   guidAsByte;
        [SerializeField] bool   guidAsByteAssigned;
*/
        [SerializeField] string guidAsString;
    	System.Guid _guid;
        public bool m_autoAssign = true;
    	public bool m_prefabAssignment = false;
    	
        public System.Guid guid {
            get {
                if ( _guid == System.Guid.Empty ) {
                    if( !System.String.IsNullOrEmpty( guidAsString ) ) {
                        _guid = new System.Guid( guidAsString );
                    }
                }
                return _guid;
            }
        }

    	public void Reset() {
    		_guid = System.Guid.Empty;
    		guidAsString = "";
    	}
    	public void Generate() {
    		_guid = System.Guid.NewGuid();
    		guidAsString = guid.ToString();
    	}

    	public bool GuidIsEmpty(){
    		// Check for duplicates?
    		return ( guid == System.Guid.Empty );
    	}

        public void MatchGuidSource(AtRng.MobileTTA.GuidMB other) {
            _guid        = other._guid;
            guidAsString = guid.ToString();
/*
            guidAsByte   = other.GetByteID();
            guidAsByteAssigned = other.ByteIDAssigned();
*/
        }
        
        public string ShortString()
        {
            string convert = guid.ToString();
            convert = convert.Remove(5);
            return convert;
        }

/*
        public void UnAssignByteID() {
            guidAsByteAssigned = false;
        }
        
        public void AssignByteID(byte byteValue) {
            if(!guidAsByteAssigned)
            {
                guidAsByte = byteValue;
                guidAsByteAssigned = true;
            }
        }
        public bool ByteIDAssigned() {
            return guidAsByteAssigned;
        }
        public byte GetByteID() {
            return guidAsByte;
        }
        public void GenerateCharID()
        {
            if(!guidAsCharAssigned)
            {
                guidAsCharAssigned = true;
            }
        }
        public void UnAssignCharID()
        {
            guidAsCharAssigned = false;
        }
*/
    }
}
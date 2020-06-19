using UnityEngine;
using System.Collections;

namespace AtRng.MobileTTA {
    public class GuidMB : MonoBehaviour {
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
        }

        public string ShortString() {
            string convert = guid.ToString();
            convert = convert.Remove(5);
            return convert;
        }

    }
}
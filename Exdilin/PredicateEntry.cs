using System;
using Blocks;

namespace Exdilin {
	public enum PredicateType {
		NORMAL,
		BLOCK_VARIABLE_OPERATION,
		GLOBAL_VARIABLE_OPERATION,
		BLOCK_VARIABLE_OPERATION_ON_OTHER
	}

	public class PredicateEntry {

		public string id;
		public PredicateSensorConstructorDelegate sensorConstructor;
		public PredicateActionConstructorDelegate actionConstructor;
		public PredicateType predicateType;
		public Type blockType = typeof(Block);
		public Type[] argTypes;
		public string[] argNames;

		/// <summary>
		/// Default value for BLOCK_VARIABLE_OPERATION and GLOBAL_VARIABLE_OPERATION predicates.
		/// </summary>
		public int variableDefault;

		/// <summary>
		/// Default value for BLOCK_VARIABLE_OPERATION_ON_OTHER predicates.
		/// </summary>
		public string variableLabel;

		public PredicateEntry(string id) {
			this.id = id;
		}

		public PredicateEntry(string id, PredicateSensorDelegate sensor) {
			this.id = id;
			SetSensorDelegate(sensor);
		}

		public PredicateEntry(string id, PredicateActionDelegate action) {
			this.id = id;
			SetActionDelegate(action);
		}

		public PredicateEntry(string id, PredicateSensorDelegate sensor, PredicateActionDelegate action) {
			this.id = id;
			SetSensorDelegate(sensor);
			SetActionDelegate(action);
		}

		public void SetActionDelegate(PredicateActionDelegate action) {
			actionConstructor = (Block b) => action;
		}

		public void SetSensorDelegate(PredicateSensorDelegate sensor) {
			sensorConstructor = (Block b) => sensor;
		}

		public void register() {
			PredicateEntryRegistry.AddPredicate(this);
		}
	}
}


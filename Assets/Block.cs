using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block {

	public static Block Create(Type type) {
		Block newBlock = new Block();
		newBlock.type = type;
		return newBlock;
	}

	public enum Type {
		Dirt
	};

	Type type;
}

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace RogueLibsCore
{
	public class RogueSprite
	{
		private Texture2D texture;
		public Texture2D Texture
		{
			get => texture;
			set
			{
				bool defined = IsDefined || isPrepared;
				if (defined && value is null) throw new ArgumentNullException(nameof(value));
				Undefine();
				texture = value;
				value.filterMode = FilterMode.Point;
				if (value != null) value.name = Name;
				Material = null;
				LightUpMaterial = null;
				Sprite = CreateSprite();
				if (defined) Define();
			}
		}
		private float pixelsPerUnit;
		public float PixelsPerUnit
		{
			get => pixelsPerUnit;
			set
			{
				bool defined = IsDefined || isPrepared;
				Undefine();
				pixelsPerUnit = value;
				Material = null;
				LightUpMaterial = null;
				Sprite = CreateSprite();
				if (defined) Define();
			}
		}
		private string name;
		public string Name
		{
			get => name;
			set
			{
				bool defined = IsDefined || isPrepared;
				if (defined && value is null) throw new ArgumentNullException(nameof(value));
				Undefine();
				name = value;
				if (Texture != null) Texture.name = value;
				if (Sprite != null) Sprite.name = value;
				if (defined) Define();
			}
		}
		private SpriteScope scope;
		public SpriteScope Scope
		{
			get => scope;
			set
			{
				bool defined = IsDefined || isPrepared;
				Undefine();
				scope = value;
				if (defined) Define();
			}
		}
		private Rect? region;
		public Rect? Region
		{
			get => region;
			set
			{
				bool defined = IsDefined || isPrepared;
				Undefine();
				region = value;
				Material = null;
				LightUpMaterial = null;
				Sprite = CreateSprite();
				if (defined) Define();
			}
		}

		private List<CustomTk2dDefinition> definitions;
		public bool IsDefined
		{
			get => definitions != null;
			set
			{
				if (value) Define();
				else Undefine();
			}
		}

		public Sprite Sprite { get; private set; }
		private Sprite CreateSprite()
		{
			if (Texture is null || Name is null || PixelsPerUnit <= 0f) return null;
			Rect reg = Region ?? new Rect(0, 0, Texture.width, Texture.height);
			Sprite sprite = Sprite.Create(Texture, reg, reg.size / 2f, PixelsPerUnit);
			sprite.name = Name;
			return sprite;
		}

		internal static readonly Dictionary<SpriteScope, List<RogueSprite>> prepared = new Dictionary<SpriteScope, List<RogueSprite>>
		{
			[SpriteScope.Items] = new List<RogueSprite>(),
			[SpriteScope.Objects] = new List<RogueSprite>(),
			[SpriteScope.Floors] = new List<RogueSprite>()
		};
		internal bool isPrepared;
		internal static void DefinePrepared(tk2dSpriteCollectionData collection, SpriteScope scope)
		{
			if (prepared.TryGetValue(scope, out List<RogueSprite> sprites))
				foreach (RogueSprite sprite in sprites)
				{
					sprite.isPrepared = false;
					sprite.Define(collection, scope);
				}
		}

		internal RogueSprite(string spriteName, SpriteScope spriteScope, byte[] rawData, Rect? spriteRegion, float ppu = 64f)
		{
			texture = new Texture2D(13, 6) { name = name = spriteName, filterMode = FilterMode.Point };
			scope = spriteScope;
			texture.LoadImage(rawData);
			pixelsPerUnit = ppu;
			region = spriteRegion;
			Sprite = CreateSprite();
		}

		private static tk2dSpriteCollectionData GetCollection(SpriteScope scope)
		{
			GameController gc = GameController.gameController;
			switch (scope)
			{
				case SpriteScope.Items: return gc?.spawnerMain?.itemSprites;
				case SpriteScope.Objects: return gc?.spawnerMain?.objectSprites;
				case SpriteScope.Floors: return gc?.spawnerMain?.floorSprites;
				default: return null;
			}
		}
		public void Define()
		{
			if (IsDefined || isPrepared || Scope == SpriteScope.None) return;
			if (Name is null) throw new InvalidOperationException($"{nameof(Name)} must not be null.");
			if (Texture is null) throw new InvalidOperationException($"{nameof(Texture)} must not be null.");
			if (PixelsPerUnit <= 0f) throw new InvalidOperationException($"{nameof(PixelsPerUnit)} must be greater than 0.");

			definitions = new List<CustomTk2dDefinition>();
			DefineScope(Scope & SpriteScope.Items);
			DefineScope(Scope & SpriteScope.Objects);
			DefineScope(Scope & SpriteScope.Floors);
		}
		private void DefineScope(SpriteScope scope)
		{
			if (scope == SpriteScope.None) return;
			tk2dSpriteCollectionData coll = GetCollection(scope);
			if (coll is null)
			{
				if (prepared.TryGetValue(scope, out List<RogueSprite> sprites))
					sprites.Add(this);
				isPrepared = true;
			}
			else Define(coll, scope);
		}

		internal Material Material { get; private set; }
		internal Material LightUpMaterial { get; private set; }
		internal void Define(tk2dSpriteCollectionData coll, SpriteScope scope)
		{
			if (coll != null)
			{
				tk2dSpriteDefinition def = AddDefinition(coll, texture, region);
				def.__RogueLibsCustom = this;
				definitions.Add(new CustomTk2dDefinition(coll, def));

				if (Material is null) Material = def.material;
				if (LightUpMaterial is null) LightUpMaterial = UnityEngine.Object.Instantiate(Material);
				LightUpMaterial.shader = GameController.gameController.lightUpShader;
			}

			GameResources gr = GameResources.gameResources;

			if (scope == SpriteScope.Items) { gr.itemDic[Name] = Sprite; gr.itemList.Add(Sprite); }
			else if (scope == SpriteScope.Objects) { gr.objectDic[Name] = Sprite; gr.objectList.Add(Sprite); }
			else if (scope == SpriteScope.Floors) { gr.floorDic[Name] = Sprite; gr.floorList.Add(Sprite); }
			else if (scope == SpriteScope.Extra) RogueFramework.ExtraSprites[Name] = Sprite;
		}
		public void Undefine()
		{
			if (isPrepared)
			{
				if (prepared.TryGetValue(Scope, out List<RogueSprite> list))
					list.Remove(this);
				isPrepared = false;
				return;
			}
			if (!IsDefined) return;

			foreach (CustomTk2dDefinition def in definitions)
				RemoveDefinition(def.Collection, def.Definition);
			definitions = null;

			GameResources gr = GameResources.gameResources;

			if (Scope == SpriteScope.Items) { gr.itemDic.Remove(Name); gr.itemList.Remove(Sprite); }
			else if (Scope == SpriteScope.Objects) { gr.objectDic.Remove(Name); gr.objectList.Remove(Sprite); }
			else if (Scope == SpriteScope.Floors) { gr.floorDic.Remove(Name); gr.floorList.Remove(Sprite); }
			else if (Scope == SpriteScope.Extra) RogueFramework.ExtraSprites.Remove(Name);
		}

		public static tk2dSpriteDefinition CreateDefinition(Texture2D texture, Rect? region, float scale)
		{
			if (texture is null) throw new ArgumentNullException(nameof(texture));
			if (scale <= 0f) throw new ArgumentOutOfRangeException(nameof(scale), scale, $"{nameof(scale)} must be greater than 0.");

			float x = texture.width;
			float y = texture.height;
			Rect uvRegion = region ?? new Rect(0f, 0f, x, y);
			Vector2 anchor = new Vector2(0.5f * x, 0.5f * y);
			Rect trimRect = new Rect(0f, 0f, 0f, 0f);

			Vector2 vector = new Vector2(0.001f, 0.001f);
			Vector2 vector2 = new Vector2((uvRegion.x + vector.x) / x, 1f - (uvRegion.y + uvRegion.height + vector.y) / y);
			Vector2 vector3 = new Vector2((uvRegion.x + uvRegion.width - vector.x) / x, 1f - (uvRegion.y - vector.y) / y);
			Vector2 vector4 = new Vector2(trimRect.x - anchor.x, -trimRect.y + anchor.y) * scale;
			Vector3 vector5 = new Vector3(-anchor.x * scale, anchor.y * scale, 0f);
			Vector3 vector6 = vector5 + new Vector3(trimRect.width * scale, -trimRect.height * scale, 0f);
			Vector3 vector7 = new Vector3(0f, -y * scale, 0f);
			Vector3 vector8 = vector7 + new Vector3(x * scale, y * scale, 0f);

			Vector3 b = new Vector3(vector5.x, vector6.y, 0f);
			Vector3 a = new Vector3(vector6.x, vector5.y, 0f);

			return new tk2dSpriteDefinition
			{
				name = texture.name,
				material = new Material(Shader.Find("tk2d/BlendVertexColor")) { name = texture.name, mainTexture = texture },
				normals = new Vector3[0],
				tangents = new Vector4[0],
				indices = new int[6] { 0, 3, 1, 2, 3, 0 },
				positions = new Vector3[]
				{
					new Vector3(vector7.x + vector4.x, vector7.y + vector4.y, 0f),
					new Vector3(vector8.x + vector4.x, vector7.y + vector4.y, 0f),
					new Vector3(vector7.x + vector4.x, vector8.y + vector4.y, 0f),
					new Vector3(vector8.x + vector4.x, vector8.y + vector4.y, 0f)
				},
				uvs = new Vector2[]
				{
					new Vector2(vector2.x, vector2.y),
					new Vector2(vector3.x, vector2.y),
					new Vector2(vector2.x, vector3.y),
					new Vector2(vector3.x, vector3.y)
				},
				boundsData = new Vector3[] { (a + b) / 2f, a - b },
				untrimmedBoundsData = new Vector3[] { (a + b) / 2f, a - b },
				texelSize = new Vector2(scale, scale)
			};
		}
		private static tk2dSpriteDefinition AddDefinition(tk2dSpriteCollectionData coll, Texture2D texture, Rect? region)
		{
			tk2dSpriteDefinition definition = CreateDefinition(texture, region, 1f / coll.invOrthoSize / coll.halfTargetHeight);

			tk2dSpriteDefinition[] newDefs = new tk2dSpriteDefinition[coll.spriteDefinitions.Length + 1];
			Array.Copy(coll.spriteDefinitions, 0, newDefs, 0, coll.spriteDefinitions.Length);
			newDefs[newDefs.Length - 1] = definition;
			coll.spriteDefinitions = newDefs;

			Material[] newMats = new Material[coll.materials.Length + 1];
			Array.Copy(coll.materials, 0, newMats, 0, coll.materials.Length);
			newMats[newMats.Length - 1] = definition.material;
			coll.materials = newMats;

			Texture[] newTextures = new Texture[coll.textures.Length + 1];
			Array.Copy(coll.textures, 0, newTextures, 0, coll.textures.Length);
			newTextures[newTextures.Length - 1] = texture;
			coll.textures = newTextures;

			coll.inst.materialIdsValid = false;
			coll.InitMaterialIds();
			coll.ClearDictionary();
			coll.InitDictionary();

			return definition;
		}
		private static void RemoveDefinition(tk2dSpriteCollectionData coll, tk2dSpriteDefinition definition)
		{
			int index = Array.IndexOf(coll.spriteDefinitions, definition);
			if (index is -1) return;

			tk2dSpriteDefinition[] newDefs = new tk2dSpriteDefinition[coll.spriteDefinitions.Length - 1];
			Array.Copy(coll.spriteDefinitions, 0, newDefs, 0, index);
			Array.Copy(coll.spriteDefinitions, index + 1, newDefs, index, coll.spriteDefinitions.Length - index - 1);
			coll.spriteDefinitions = newDefs;

			Material[] newMats = new Material[coll.materials.Length - 1];
			Array.Copy(coll.materials, 0, newMats, 0, index);
			Array.Copy(coll.materials, index + 1, newMats, index, coll.materials.Length - index - 1);
			coll.materials = newMats;

			Texture[] newTextures = new Texture[coll.textures.Length - 1];
			Array.Copy(coll.textures, 0, newTextures, 0, index);
			Array.Copy(coll.textures, index + 1, newTextures, index, coll.textures.Length - index - 1);
			coll.textures = newTextures;

			coll.inst.materialIdsValid = false;
			coll.InitMaterialIds();
			coll.ClearDictionary();
			coll.InitDictionary();
			definition.__RogueLibsCustom = null;
		}

		public class CustomTk2dDefinition
		{
			internal CustomTk2dDefinition(tk2dSpriteCollectionData collection, tk2dSpriteDefinition definition)
			{
				Collection = collection;
				Definition = definition;
			}
			public tk2dSpriteCollectionData Collection { get; }
			public tk2dSpriteDefinition Definition { get; }
		}
	}
	[Flags]
	public enum SpriteScope
	{
		Extra   = 1 << 31,

		None    = 0,

		Items   = 1 << 0,
		Objects = 1 << 1,
		Floors  = 1 << 2
	}
}
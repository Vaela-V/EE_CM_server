﻿/*
EE CM serverside codes
Copyright (C) 2013-2015 Krock/SmallJoker <mk939@ymail.com>


This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using PlayerIO.GameLibrary;

namespace EE_CM {
	public class Player : BasePlayer {
		public string Name = "", last_said = "";
		public pList<string> muted = new pList<string>();
		public int Face = 0,
				   code_tries = 0,
				   coins = 0,
				   sameText = 0,
				   moved = 0,
				   mWarns = 0,
				   cPointX = -1,
				   cPointY = -1,
				   say_counter = 0,
				   system_messages = 0,

				   posX = 16,
				   posY = 16,
				   speedX = 0,
				   speedY = 0,
				   gravityX = 0,
				   gravityY = 0,
				   keyX = 0,
				   keyY = 0;
		public bool isBeta = false,
					isOwner = false,
					isGod = false,
					isMod = false,
					isModerator = false,
					isVigilant = false,
					isInited = false,
					isBot = false,
					firstFace = false,
					initWait = false,
					isGuest = false,
					canEdit = false,
					levelComplete = false,
					gotCoin = false,
					getBlockInfo = false,
					isDead = false,
					wootGiven = false;
	}

	class Block {
		public int[] posX,
			posY;
		public int used;

		public Block() {
			posX = new int[10];
			posY = new int[10];
			used = 0;
			for (int i = 0; i < posX.Length; i++) {
				posX[i] = -5;
				posY[i] = -5;
			}
		}
		public Block(int[] x, int[] y) {
			posX = x;
			posY = y;
			used = 0;
			for (int i = 0; i < x.Length; i++) {
				if (x[i] >= 0 && y[i] >= 0) {
					used++;
				}
			}
		}

		public void Set(int x, int y) {
			#region if_not_override
			if (!Contains(x, y)) {
				if (used > posX.Length - 10) {
					extendArr();
				}

				for (int c = 0; c < posX.Length; c++) {
					if (posX[c] < 0 && posY[c] < 0) {
						posX[c] = x;
						posY[c] = y;
						used++;
						break;
					}
				}
			}
			#endregion
		}

		void extendArr() {
			int extend = posX.Length;
			if (extend >= 500) {
				extend = 500;
			}
			int[] BposX = new int[posX.Length + extend],
				BposY = new int[posX.Length + extend];
			for (int i = 0; i < BposX.Length; i++) {
				if (i < posX.Length) {
					BposX[i] = posX[i];
					BposY[i] = posY[i];
				} else {
					BposX[i] = -5;
					BposY[i] = -5;
				}
			}
			posX = BposX;
			posY = BposY;
		}

		public bool Remove(int x, int y) {
			bool ret = false;
			for (int c = 0; c < posX.Length; c++) {
				if (posX[c] == x && posY[c] == y) {
					posX[c] = -5;
					posY[c] = -5;
					if (!ret) ret = true;
					used--;
				}
			}
			return ret;
		}

		public bool Contains(int x, int y) {
			bool ret = false;
			for (int c = 0; c < posX.Length; c++) {
				if (posX[c] == x && posY[c] == y) {
					ret = true;
					break;
				}
			}
			return ret;
		}
	}

	class WorldInfo {
		string[] censored = new string[] { "fuck", "bitch", "asshole", "nazi", "shit", "dick", "penis", "fvck", "gay", "sex", "porn", "bastard", "cunt", "nigger", "nigga", "pussy", "tits", "titts", "boobs", "tranny", "B==0", "O==3" };

		public Random random = new Random((int)DateTime.Now.Ticks);

		public int[] getWorldSize(int type) {
			int width = 50, //small
				height = 50;

			switch (type) {
			case 1: //medium
				width = 100; //10'000
				height = 100;
				break;
			case 2: //large
				width = 150; //22'500
				height = 150;
				break;
			case 3: //massive
				width = 200; //40'000
				height = 200;
				break;
			case 4: //wide
				width = 400; //40'000
				height = 100;
				break;
			case 5: //great
				width = 400; //80'000
				height = 200;
				break;
			}
			return new int[] { width, height };
		}

		public string check_Censored(string text) {
			char[] inp = text.ToCharArray();
			char[] inp_low = new char[inp.Length];

			for (int i = 0; i < inp.Length; i++) {
				switch (inp[i]) {
				case '\n': inp[i] = 'n'; break;
				case '\r': inp[i] = 'r'; break;
				case '\t': inp[i] = 't'; break;
				}

				char cur = char.ToLower(inp[i]);
				switch (cur) {
				case '!':
				case '1':
					cur = 'i';
					break;
				case '0': cur = 'o'; break;
				case '3': cur = 'e'; break;
				case '5':
				case '$':
					cur = 's'; break;
				case '7': cur = 't'; break;
				case '8': cur = 'b'; break;
				}
				inp_low[i] = cur;
			}

			for (int k = 0; k < censored.Length; k++) {
				int count = 0;
				for (int i = 0; i < inp.Length; i++) {
					if (inp_low[i] == censored[k][count]) {
						if (count + 1 == censored[k].Length) {
							for (int r = i - count; r <= i; r++) {
								inp[r] = '*';
							}
							count = 0;
							continue;
						}
						count++;
					} else count = 0;
				}
			}
			return new string(inp);
		}

		public int getInt(string x) {
			int o = -1;
			if (!int.TryParse(x, out o)) return -1;
			return o;
		}
	}

	struct Bindex {
		public int FG, BG, arg3, pId, pTarget, FGp, BGp;
	}

	struct COOR {
		public int x, y;
	}

	public class pList<type> {
		int count = 0;
		type[] data;
		bool[] sets;
		public pList() {
			data = new type[100];
			sets = new bool[100];
		}

		public pList(type[] values) {
			data = values;
			sets = new bool[values.Length];
			count = 0;
			for (int i = 0; i < data.Length; i++) {
				sets[i] = true;
				count++;
			}
		}

		public int Used { get { return count; } }

		public void Add(type value) {
			if (Contains(value)) return;

			int index = -1;
			for (int i = 0; i < data.Length; i++) {
				if (sets[i]) continue;
				index = i;
				break;
			}

			if (index < 0) {
				index = data.Length;
				Array.Resize(ref data, index + 100);
				Array.Resize(ref sets, index + 100);
			}

			data[index] = value;
			sets[index] = true;
			count++;
		}

		public void Add(type[] values) {
			for (int i = 0; i < values.Length; i++) {
				Add(values[i]);
			}
		}

		public void Remove(type value) {
			for (int i = 0; i < data.Length; i++) {
				if (data[i] == null || !sets[i]) {
					sets[i] = false;
					continue;
				}

				if (data[i].Equals(value)) {
					sets[i] = false;
					count--;
				}
			}
		}

		public bool Contains(type value) {
			for (int i = 0; i < data.Length; i++) {
				if (!sets[i]) continue;
				if (data[i] == null || string.IsNullOrEmpty(data[i].ToString())) {
					sets[i] = false;
					continue;
				}

				if (data[i].Equals(value)) {
					return true;
				}
			}
			return false;
		}

		public type[] GetData() {
			type[] exp_data = new type[count];
			int c = 0;
			for (int i = 0; i < data.Length && c < count; i++) {
				if (!sets[i]) continue;
				if (data[i] == null || string.IsNullOrEmpty(data[i].ToString())) {
					sets[i] = false;
					continue;
				}
				exp_data[c] = data[i];
				c++;
			}
			if (exp_data.Length > c) Array.Resize(ref exp_data, c);
			return exp_data;
		}
	}

	class PlayerHistory {
		public string Name, Id;
		public long join_time;
	}
}

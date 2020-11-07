using System;

namespace ZXMAK.Engine.Z80
{
	public class Z80CPU
	{
		public Z80CPU()
		{
			this.ALU_INIT();
			this.initExecTBL();
		}

		public void Reset()
		{
			this.RST = true;
			this.ExecCycle();
			this.RST = false;
		}

		public void ExecCycle()
		{
			if (this.OnCycle != null)
			{
				this.OnCycle();
			}
			if (this.ParseSignals())
			{
				return;
			}
			if (this.HALTED)
			{
				this.Tact += 3UL;
				this.RegenMemory();
				return;
			}
			byte b = this.ReadMemory(this.regs.PC, true);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			if (this.XFX == OPXFX.CB)
			{
				this.BlockINT = false;
				this.ExecCB(b);
				this.XFX = OPXFX.NONE;
				this.FX = OPFX.NONE;
				return;
			}
			if (this.XFX == OPXFX.ED)
			{
				this.RegenMemory();
				this.BlockINT = false;
				this.ExecED(b);
				this.XFX = OPXFX.NONE;
				this.FX = OPFX.NONE;
				return;
			}
			if (b == 221)
			{
				this.RegenMemory();
				this.FX = OPFX.IX;
				this.BlockINT = true;
				return;
			}
			if (b == 253)
			{
				this.RegenMemory();
				this.FX = OPFX.IY;
				this.BlockINT = true;
				return;
			}
			if (b == 203)
			{
				this.RegenMemory();
				this.XFX = OPXFX.CB;
				this.BlockINT = true;
				return;
			}
			if (b == 237)
			{
				this.RegenMemory();
				this.XFX = OPXFX.ED;
				this.BlockINT = true;
				return;
			}
			this.RegenMemory();
			this.BlockINT = false;
			this.ExecDirect(b);
			this.FX = OPFX.NONE;
		}

		private void ExecED(byte cmd)
		{
			Z80CPU.XFXOPDO xfxopdo = this.edopTABLE[(int)cmd];
			if (xfxopdo != null)
			{
				xfxopdo(cmd);
			}
		}

		private void ExecCB(byte cmd)
		{
			if (this.FX != OPFX.NONE)
			{
				if (this.FX == OPFX.IX)
				{
					this.regs.MW = this.regs.IX + (ushort)((sbyte)cmd);
				}
				else
				{
					this.regs.MW = this.regs.IY + (ushort)((sbyte)cmd);
				}
				this.Tact += 1UL;
				cmd = this.ReadMemory(this.regs.PC, true);
				this.Tact += 3UL;
				Registers registers = this.regs;
				registers.PC += 1;
				this.fxcbopTABLE[(int)cmd](cmd, this.regs.MW);
				return;
			}
			this.cbopTABLE[(int)cmd](cmd);
			this.RegenMemory();
		}

		private void ExecDirect(byte cmd)
		{
			Z80CPU.XFXOPDO xfxopdo;
			if (this.FX == OPFX.NONE)
			{
				xfxopdo = this.opTABLE[(int)cmd];
			}
			else
			{
				xfxopdo = this.fxopTABLE[(int)cmd];
			}
			if (xfxopdo != null)
			{
				xfxopdo(cmd);
			}
		}

		private void RegenMemory()
		{
			this.regs.R = ((this.regs.R + 1 & 127) | (this.regs.R & 128));
			this.Tact += 1UL;
		}

		private bool ParseSignals()
		{
			if (this.RST)
			{
				this.regs.PC = 0;
				this.regs.IR = 0;
				this.IFF1 = false;
				this.IFF2 = false;
				this.IM = 0;
				this.HALTED = false;
				this.FX = OPFX.NONE;
				this.XFX = OPXFX.NONE;
				this.Tact += 4UL;
				return true;
			}
			if (this.NMI)
			{
				this.IFF1 = false;
				this.HALTED = false;
				Registers registers = this.regs;
				registers.SP -= 1;
				this.Tact += 1UL;
				this.WriteMemory(this.regs.SP, (byte)(this.regs.PC >> 8));
				this.Tact += 3UL;
				Registers registers2 = this.regs;
				registers2.SP -= 1;
				this.WriteMemory(this.regs.SP, (byte)(this.regs.PC & 255));
				this.Tact += 3UL;
				this.regs.PC = 102;
				return true;
			}
			if (this.INT && !this.BlockINT && this.IFF1)
			{
				this.IFF1 = false;
				this.IFF2 = false;
				this.HALTED = false;
				Registers registers3 = this.regs;
				registers3.SP -= 1;
				this.Tact += 1UL;
				this.WriteMemory(this.regs.SP, (byte)(this.regs.PC >> 8));
				this.Tact += 3UL;
				Registers registers4 = this.regs;
				registers4.SP -= 1;
				this.WriteMemory(this.regs.SP, (byte)(this.regs.PC & 255));
				this.Tact += 3UL;
				if (this.IM == 0)
				{
					this.regs.MW = 56;
					this.Tact += 5UL;
				}
				else if (this.IM == 1)
				{
					this.regs.MW = 56;
					this.Tact += 5UL;
				}
				else
				{
					ushort num = (ushort)this.FreeBUS;
					num += (this.regs.IR & 65280);
					this.regs.MW = (ushort)this.ReadMemory(num, true);
					this.Tact += 3UL;
					Registers registers5 = this.regs;
					registers5.MW += (ushort)((int)this.ReadMemory(num + 1, true) * 256);
					this.Tact += 3UL;
					this.Tact += 6UL;
				}
				this.regs.PC = this.regs.MW;
				return true;
			}
			return false;
		}

		private void initExecTBL()
		{
			this.initExec();
			this.initExecFX();
			this.initExecED();
			this.initExecCB();
		}

		private void ED_LDI(byte cmd)
		{
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) + 1;
			byte b = readMemory(hl, false);
			this.Tact += 4UL;
			OnWRMEM writeMemory = this.WriteMemory;
			Registers registers2 = this.regs;
			ushort de;
			registers2.DE = (de = registers2.DE) + 1;
			writeMemory(de, b);
			this.Tact += 4UL;
			b += this.regs.A;
			b = (byte)((int)(b & 8) + ((int)b << 4 & 32));
			this.regs.F = (this.regs.F & 193) + b;
			Registers registers3 = this.regs;
			if ((registers3.BC -= 1) != 0)
			{
				Registers registers4 = this.regs;
				registers4.F |= 4;
			}
		}

		private void ED_CPI(byte cmd)
		{
			byte b = this.regs.F & 1;
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) + 1;
			byte b2 = readMemory(hl, false);
			this.Tact += 4UL;
			this.regs.F = this.cpf8b[(int)this.regs.A * 256 + (int)b2] + b;
			this.Tact += 4UL;
			Registers registers2 = this.regs;
			if ((registers2.BC -= 1) != 0)
			{
				Registers registers3 = this.regs;
				registers3.F |= 4;
			}
			Registers registers4 = this.regs;
			registers4.MW += 1;
		}

		private void ED_INI(byte cmd)
		{
			this.regs.MW = this.regs.BC + 1;
			byte value = this.ReadPort(this.regs.BC);
			this.Tact += 4UL;
			OnWRMEM writeMemory = this.WriteMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) + 1;
			writeMemory(hl, value);
			this.Tact += 3UL;
			this.regs.B = this.ALU_DECR(this.regs.B);
			this.Tact += 1UL;
		}

		private void ED_OUTI(byte cmd)
		{
			this.regs.B = this.ALU_DECR(this.regs.B);
			this.Tact += 1UL;
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) + 1;
			byte value = readMemory(hl, false);
			this.Tact += 3UL;
			this.WritePort(this.regs.BC, value);
			this.Tact += 4UL;
			Registers registers2 = this.regs;
			registers2.F &= 254;
			if (this.regs.L == 0)
			{
				Registers registers3 = this.regs;
				registers3.F |= 1;
			}
			this.regs.MW = this.regs.BC + 1;
		}

		private void ED_LDD(byte cmd)
		{
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) - 1;
			byte b = readMemory(hl, false);
			this.Tact += 3UL;
			OnWRMEM writeMemory = this.WriteMemory;
			Registers registers2 = this.regs;
			ushort de;
			registers2.DE = (de = registers2.DE) - 1;
			writeMemory(de, b);
			this.Tact += 3UL;
			b += this.regs.A;
			b = (byte)((int)(b & 8) + ((int)b << 4 & 32));
			this.regs.F = (this.regs.F & 193) + b;
			Registers registers3 = this.regs;
			if ((registers3.BC -= 1) != 0)
			{
				Registers registers4 = this.regs;
				registers4.F |= 4;
			}
			this.Tact += 2UL;
		}

		private void ED_CPD(byte cmd)
		{
			byte b = this.regs.F & 1;
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) - 1;
			byte b2 = readMemory(hl, false);
			this.Tact += 4UL;
			this.regs.F = this.cpf8b[(int)this.regs.A * 256 + (int)b2] + b;
			Registers registers2 = this.regs;
			if ((registers2.BC -= 1) != 0)
			{
				Registers registers3 = this.regs;
				registers3.F |= 4;
			}
			this.Tact += 4UL;
			Registers registers4 = this.regs;
			registers4.MW -= 1;
		}

		private void ED_IND(byte cmd)
		{
			this.regs.MW = this.regs.BC - 1;
			byte value = this.ReadPort(this.regs.BC);
			this.Tact += 4UL;
			OnWRMEM writeMemory = this.WriteMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) - 1;
			writeMemory(hl, value);
			this.Tact += 3UL;
			this.regs.B = this.ALU_DECR(this.regs.B);
			this.Tact += 1UL;
		}

		private void ED_OUTD(byte cmd)
		{
			this.regs.B = this.ALU_DECR(this.regs.B);
			this.Tact += 1UL;
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) - 1;
			byte value = readMemory(hl, false);
			this.Tact += 3UL;
			this.WritePort(this.regs.BC, value);
			this.Tact += 4UL;
			Registers registers2 = this.regs;
			registers2.F &= 254;
			if (this.regs.L == 255)
			{
				Registers registers3 = this.regs;
				registers3.F |= 1;
			}
			this.regs.MW = this.regs.BC - 1;
		}

		private void ED_LDIR(byte cmd)
		{
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) + 1;
			byte b = readMemory(hl, false);
			this.Tact += 3UL;
			OnWRMEM writeMemory = this.WriteMemory;
			Registers registers2 = this.regs;
			ushort de;
			registers2.DE = (de = registers2.DE) + 1;
			writeMemory(de, b);
			this.Tact += 3UL;
			b += this.regs.A;
			b = (byte)((int)(b & 8) + ((int)b << 4 & 32));
			this.regs.F = (this.regs.F & 193) + b;
			this.Tact += 2UL;
			Registers registers3 = this.regs;
			if ((registers3.BC -= 1) != 0)
			{
				Registers registers4 = this.regs;
				registers4.F |= 4;
				Registers registers5 = this.regs;
				registers5.PC -= 2;
				this.Tact += 5UL;
				this.regs.MW = this.regs.PC + 1;
			}
		}

		private void ED_CPIR(byte cmd)
		{
			Registers registers = this.regs;
			registers.MW += 1;
			byte b = this.regs.F & 1;
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers2 = this.regs;
			ushort hl;
			registers2.HL = (hl = registers2.HL) + 1;
			byte b2 = readMemory(hl, false);
			this.Tact += 3UL;
			this.regs.F = this.cpf8b[(int)this.regs.A * 256 + (int)b2] + b;
			this.Tact += 5UL;
			Registers registers3 = this.regs;
			if ((registers3.BC -= 1) != 0)
			{
				Registers registers4 = this.regs;
				registers4.F |= 4;
				if ((this.regs.F & 64) == 0)
				{
					Registers registers5 = this.regs;
					registers5.PC -= 2;
					this.Tact += 5UL;
					this.regs.MW = this.regs.PC + 1;
				}
			}
		}

		private void ED_INIR(byte cmd)
		{
			this.regs.MW = this.regs.BC + 1;
			byte value = this.ReadPort(this.regs.BC);
			this.Tact += 4UL;
			OnWRMEM writeMemory = this.WriteMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) + 1;
			writeMemory(hl, value);
			this.Tact += 4UL;
			this.regs.B = this.ALU_DECR(this.regs.B);
			if (this.regs.B != 0)
			{
				Registers registers2 = this.regs;
				registers2.F |= 4;
				Registers registers3 = this.regs;
				registers3.PC -= 2;
				this.Tact += 5UL;
				return;
			}
			Registers registers4 = this.regs;
			registers4.F &= 251;
		}

		private void ED_OTIR(byte cmd)
		{
			this.regs.B = this.ALU_DECR(this.regs.B);
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) + 1;
			byte value = readMemory(hl, false);
			this.Tact += 4UL;
			this.WritePort(this.regs.BC, value);
			this.Tact += 4UL;
			if (this.regs.B != 0)
			{
				Registers registers2 = this.regs;
				registers2.F |= 4;
				Registers registers3 = this.regs;
				registers3.PC -= 2;
				this.Tact += 5UL;
			}
			else
			{
				Registers registers4 = this.regs;
				registers4.F &= 251;
			}
			Registers registers5 = this.regs;
			registers5.F &= 254;
			if (this.regs.L == 0)
			{
				Registers registers6 = this.regs;
				registers6.F |= 1;
			}
			this.regs.MW = this.regs.BC + 1;
		}

		private void ED_LDDR(byte cmd)
		{
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) - 1;
			byte b = readMemory(hl, false);
			this.Tact += 4UL;
			OnWRMEM writeMemory = this.WriteMemory;
			Registers registers2 = this.regs;
			ushort de;
			registers2.DE = (de = registers2.DE) - 1;
			writeMemory(de, b);
			this.Tact += 4UL;
			b += this.regs.A;
			b = (byte)((int)(b & 8) + ((int)b << 4 & 32));
			this.regs.F = (this.regs.F & 193) + b;
			Registers registers3 = this.regs;
			if ((registers3.BC -= 1) != 0)
			{
				Registers registers4 = this.regs;
				registers4.F |= 4;
				Registers registers5 = this.regs;
				registers5.PC -= 2;
				this.Tact += 5UL;
			}
		}

		private void ED_CPDR(byte cmd)
		{
			Registers registers = this.regs;
			registers.MW -= 1;
			byte b = this.regs.F & 1;
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers2 = this.regs;
			ushort hl;
			registers2.HL = (hl = registers2.HL) - 1;
			byte b2 = readMemory(hl, false);
			this.Tact += 4UL;
			this.regs.F = this.cpf8b[(int)this.regs.A * 256 + (int)b2] + b;
			this.Tact += 4UL;
			Registers registers3 = this.regs;
			if ((registers3.BC -= 1) != 0)
			{
				Registers registers4 = this.regs;
				registers4.F |= 4;
				if ((this.regs.F & 64) == 0)
				{
					Registers registers5 = this.regs;
					registers5.PC -= 2;
					this.Tact += 5UL;
					this.regs.MW = this.regs.PC + 1;
				}
			}
		}

		private void ED_INDR(byte cmd)
		{
			this.regs.MW = this.regs.BC - 1;
			byte value = this.ReadPort(this.regs.BC);
			this.Tact += 4UL;
			OnWRMEM writeMemory = this.WriteMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) - 1;
			writeMemory(hl, value);
			this.Tact += 4UL;
			this.regs.B = this.ALU_DECR(this.regs.B);
			if (this.regs.B != 0)
			{
				Registers registers2 = this.regs;
				registers2.F |= 4;
				Registers registers3 = this.regs;
				registers3.PC -= 2;
				this.Tact += 5UL;
				return;
			}
			Registers registers4 = this.regs;
			registers4.F &= 251;
		}

		private void ED_OTDR(byte cmd)
		{
			this.regs.B = this.ALU_DECR(this.regs.B);
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort hl;
			registers.HL = (hl = registers.HL) - 1;
			byte value = readMemory(hl, false);
			this.Tact += 4UL;
			this.WritePort(this.regs.BC, value);
			this.Tact += 4UL;
			if (this.regs.B != 0)
			{
				Registers registers2 = this.regs;
				registers2.F |= 4;
				Registers registers3 = this.regs;
				registers3.PC -= 2;
				this.Tact += 5UL;
			}
			else
			{
				Registers registers4 = this.regs;
				registers4.F &= 251;
			}
			Registers registers5 = this.regs;
			registers5.F &= 254;
			if (this.regs.L == 255)
			{
				Registers registers6 = this.regs;
				registers6.F |= 1;
			}
			this.regs.MW = this.regs.BC - 1;
		}

		private void ED_INRC(byte cmd)
		{
			this.regs.MW = this.regs.BC + 1;
			byte b = this.ReadPort(this.regs.BC);
			this.Tact += 4UL;
			int num = (cmd & 56) >> 3;
			if (num != 6)
			{
				this.regs[num] = b;
			}
			this.regs.F = (this.log_f[(int)b] | (this.regs.F & 1));
		}

		private void ED_OUTCR(byte cmd)
		{
			this.regs.MW = this.regs.BC + 1;
			int num = (cmd & 56) >> 3;
			this.Tact += 3UL;
			if (num != 6)
			{
				this.WritePort(this.regs.BC, this.regs[num]);
			}
			else
			{
				this.WritePort(this.regs.BC, 0);
			}
			this.Tact += 1UL;
		}

		private void ED_ADCHLRR(byte cmd)
		{
			this.regs.MW = this.regs.HL + 1;
			int rr = (cmd & 48) >> 4;
			byte b = (byte)((this.regs.HL & 4095) + (this.regs.GetPair(rr) & 4095) + (ushort)(this.regs.F & 1) >> 8 & 16);
			uint num = (uint)((this.regs.HL & ushort.MaxValue) + (this.regs.GetPair(rr) & ushort.MaxValue) + (ushort)(this.regs.F & 1));
			if ((num & 65536U) != 0U)
			{
				b |= 1;
			}
			if ((num & 65535U) == 0U)
			{
				b |= 64;
			}
			int num2 = (int)((short)this.regs.HL + (short)this.regs.GetPair(rr) + (short)(this.regs.F & 1));
			if (num2 < -32768 || num2 >= 32768)
			{
				b |= 4;
			}
			this.regs.HL = (ushort)num;
			this.regs.F = (b | (this.regs.H & 168));
			this.Tact += 7UL;
		}

		private void ED_SBCHLRR(byte cmd)
		{
			this.regs.MW = this.regs.HL + 1;
			int rr = (cmd & 48) >> 4;
			byte b = 2;
			b |= (byte)((this.regs.HL & 4095) - (this.regs.GetPair(rr) & 4095) - (ushort)(this.regs.F & 1) >> 8 & 16);
			uint num = (uint)((this.regs.HL & ushort.MaxValue) - (this.regs.GetPair(rr) & ushort.MaxValue) - (ushort)(this.regs.F & 1));
			if ((num & 65536U) != 0U)
			{
				b |= 1;
			}
			if ((num & 65535U) == 0U)
			{
				b |= 64;
			}
			int num2 = (int)((short)this.regs.HL - (short)this.regs.GetPair(rr) - (short)(this.regs.F & 1));
			if (num2 < -32768 || num2 >= 32768)
			{
				b |= 4;
			}
			this.regs.HL = (ushort)num;
			this.regs.F = (b | (this.regs.H & 168));
			this.Tact += 7UL;
		}

		private void ED_LDRR_NN_(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			this.regs.MW = num + 1;
			ushort num2 = (ushort)this.ReadMemory(num, false);
			this.Tact += 3UL;
			num2 += (ushort)((int)this.ReadMemory(this.regs.MW, false) * 256);
			this.Tact += 3UL;
			this.regs.SetPair((cmd & 48) >> 4, num2);
		}

		private void ED_LD_NN_RR(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			this.regs.MW = num + 1;
			ushort pair = this.regs.GetPair((cmd & 48) >> 4);
			this.WriteMemory(num, (byte)(pair & 255));
			this.Tact += 3UL;
			this.WriteMemory(this.regs.MW, (byte)(pair >> 8));
			this.Tact += 3UL;
		}

		private void ED_RETN(byte cmd)
		{
			this.IFF1 = this.IFF2;
			ushort num = (ushort)this.ReadMemory(this.regs.SP, false);
			this.Tact += 3UL;
			ushort num2 = num;
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			num = num2 + (ushort)((int)readMemory(registers.SP += 1, false) * 256);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.SP += 1;
			this.regs.PC = num;
			this.regs.MW = num;
		}

		private void ED_IM(byte cmd)
		{
			byte b = (byte)((cmd & 24) >> 3);
			if (b < 2)
			{
				b = 1;
			}
			b -= 1;
			this.IM = b;
		}

		private void ED_LDXRA(byte cmd)
		{
			if ((cmd & 8) == 0)
			{
				this.regs.I = this.regs.A;
			}
			else
			{
				this.regs.R = this.regs.A;
			}
			this.Tact += 1UL;
		}

		private void ED_LDAXR(byte cmd)
		{
			bool flag = (cmd & 8) == 0;
			if (flag)
			{
				this.regs.A = this.regs.I;
			}
			else
			{
				this.regs.A = this.regs.R;
			}
			this.regs.F = (byte)((int)(this.log_f[(int)this.regs.A] | (this.regs.F & 1)) & -5);
			if (flag)
			{
				if (this.IFF1 && !this.INT)
				{
					Registers registers = this.regs;
					registers.F |= 4;
				}
			}
			else if (this.IFF2 && !this.INT)
			{
				Registers registers2 = this.regs;
				registers2.F |= 4;
			}
			this.Tact += 1UL;
		}

		private void ED_RRD(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.regs.MW = this.regs.HL + 1;
			this.WriteMemory(this.regs.HL, (byte)((int)this.regs.A << 4 | b >> 4));
			this.Tact += 3UL;
			this.Tact += 4UL;
			this.regs.A = ((this.regs.A & 240) | (b & 15));
			this.regs.F = (this.log_f[(int)this.regs.A] | (this.regs.F & 1));
		}

		private void ED_RLD(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.regs.MW = this.regs.HL + 1;
			this.WriteMemory(this.regs.HL, (byte)((int)(this.regs.A & 15) | (int)b << 4));
			this.Tact += 3UL;
			this.Tact += 4UL;
			this.regs.A = (byte)((int)(this.regs.A & 240) | b >> 4);
			this.regs.F = (this.log_f[(int)this.regs.A] | (this.regs.F & 1));
		}

		private void ED_NEG(byte cmd)
		{
			this.regs.F = this.sbcf[(int)this.regs.A];
			this.regs.A = -this.regs.A;
		}

		private void initExecED()
		{
			Z80CPU.XFXOPDO[] array = new Z80CPU.XFXOPDO[256];
			array[64] = new Z80CPU.XFXOPDO(this.ED_INRC);
			array[65] = new Z80CPU.XFXOPDO(this.ED_OUTCR);
			array[66] = new Z80CPU.XFXOPDO(this.ED_SBCHLRR);
			array[67] = new Z80CPU.XFXOPDO(this.ED_LD_NN_RR);
			array[68] = new Z80CPU.XFXOPDO(this.ED_NEG);
			array[69] = new Z80CPU.XFXOPDO(this.ED_RETN);
			array[70] = new Z80CPU.XFXOPDO(this.ED_IM);
			array[71] = new Z80CPU.XFXOPDO(this.ED_LDXRA);
			array[72] = new Z80CPU.XFXOPDO(this.ED_INRC);
			array[73] = new Z80CPU.XFXOPDO(this.ED_OUTCR);
			array[74] = new Z80CPU.XFXOPDO(this.ED_ADCHLRR);
			array[75] = new Z80CPU.XFXOPDO(this.ED_LDRR_NN_);
			array[76] = new Z80CPU.XFXOPDO(this.ED_NEG);
			array[77] = new Z80CPU.XFXOPDO(this.ED_RETN);
			array[78] = new Z80CPU.XFXOPDO(this.ED_IM);
			array[79] = new Z80CPU.XFXOPDO(this.ED_LDXRA);
			array[80] = new Z80CPU.XFXOPDO(this.ED_INRC);
			array[81] = new Z80CPU.XFXOPDO(this.ED_OUTCR);
			array[82] = new Z80CPU.XFXOPDO(this.ED_SBCHLRR);
			array[83] = new Z80CPU.XFXOPDO(this.ED_LD_NN_RR);
			array[84] = new Z80CPU.XFXOPDO(this.ED_NEG);
			array[85] = new Z80CPU.XFXOPDO(this.ED_RETN);
			array[86] = new Z80CPU.XFXOPDO(this.ED_IM);
			array[87] = new Z80CPU.XFXOPDO(this.ED_LDAXR);
			array[88] = new Z80CPU.XFXOPDO(this.ED_INRC);
			array[89] = new Z80CPU.XFXOPDO(this.ED_OUTCR);
			array[90] = new Z80CPU.XFXOPDO(this.ED_ADCHLRR);
			array[91] = new Z80CPU.XFXOPDO(this.ED_LDRR_NN_);
			array[92] = new Z80CPU.XFXOPDO(this.ED_NEG);
			array[93] = new Z80CPU.XFXOPDO(this.ED_RETN);
			array[94] = new Z80CPU.XFXOPDO(this.ED_IM);
			array[95] = new Z80CPU.XFXOPDO(this.ED_LDAXR);
			array[96] = new Z80CPU.XFXOPDO(this.ED_INRC);
			array[97] = new Z80CPU.XFXOPDO(this.ED_OUTCR);
			array[98] = new Z80CPU.XFXOPDO(this.ED_SBCHLRR);
			array[99] = new Z80CPU.XFXOPDO(this.ED_LD_NN_RR);
			array[100] = new Z80CPU.XFXOPDO(this.ED_NEG);
			array[101] = new Z80CPU.XFXOPDO(this.ED_RETN);
			array[102] = new Z80CPU.XFXOPDO(this.ED_IM);
			array[103] = new Z80CPU.XFXOPDO(this.ED_RRD);
			array[104] = new Z80CPU.XFXOPDO(this.ED_INRC);
			array[105] = new Z80CPU.XFXOPDO(this.ED_OUTCR);
			array[106] = new Z80CPU.XFXOPDO(this.ED_ADCHLRR);
			array[107] = new Z80CPU.XFXOPDO(this.ED_LDRR_NN_);
			array[108] = new Z80CPU.XFXOPDO(this.ED_NEG);
			array[109] = new Z80CPU.XFXOPDO(this.ED_RETN);
			array[110] = new Z80CPU.XFXOPDO(this.ED_IM);
			array[111] = new Z80CPU.XFXOPDO(this.ED_RLD);
			array[112] = new Z80CPU.XFXOPDO(this.ED_INRC);
			array[113] = new Z80CPU.XFXOPDO(this.ED_OUTCR);
			array[114] = new Z80CPU.XFXOPDO(this.ED_SBCHLRR);
			array[115] = new Z80CPU.XFXOPDO(this.ED_LD_NN_RR);
			array[116] = new Z80CPU.XFXOPDO(this.ED_NEG);
			array[117] = new Z80CPU.XFXOPDO(this.ED_RETN);
			array[118] = new Z80CPU.XFXOPDO(this.ED_IM);
			array[120] = new Z80CPU.XFXOPDO(this.ED_INRC);
			array[121] = new Z80CPU.XFXOPDO(this.ED_OUTCR);
			array[122] = new Z80CPU.XFXOPDO(this.ED_ADCHLRR);
			array[123] = new Z80CPU.XFXOPDO(this.ED_LDRR_NN_);
			array[124] = new Z80CPU.XFXOPDO(this.ED_NEG);
			array[125] = new Z80CPU.XFXOPDO(this.ED_RETN);
			array[126] = new Z80CPU.XFXOPDO(this.ED_IM);
			array[160] = new Z80CPU.XFXOPDO(this.ED_LDI);
			array[161] = new Z80CPU.XFXOPDO(this.ED_CPI);
			array[162] = new Z80CPU.XFXOPDO(this.ED_INI);
			array[163] = new Z80CPU.XFXOPDO(this.ED_OUTI);
			array[168] = new Z80CPU.XFXOPDO(this.ED_LDD);
			array[169] = new Z80CPU.XFXOPDO(this.ED_CPD);
			array[170] = new Z80CPU.XFXOPDO(this.ED_IND);
			array[171] = new Z80CPU.XFXOPDO(this.ED_OUTD);
			array[176] = new Z80CPU.XFXOPDO(this.ED_LDIR);
			array[177] = new Z80CPU.XFXOPDO(this.ED_CPIR);
			array[178] = new Z80CPU.XFXOPDO(this.ED_INIR);
			array[179] = new Z80CPU.XFXOPDO(this.ED_OTIR);
			array[184] = new Z80CPU.XFXOPDO(this.ED_LDDR);
			array[185] = new Z80CPU.XFXOPDO(this.ED_CPDR);
			array[186] = new Z80CPU.XFXOPDO(this.ED_INDR);
			array[187] = new Z80CPU.XFXOPDO(this.ED_OTDR);
			this.edopTABLE = array;
		}

		private void INA_NN_(byte cmd)
		{
			this.Tact += 2UL;
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort pc;
			registers.PC = (pc = registers.PC) + 1;
			ushort num = (ushort)readMemory(pc, false);
			this.Tact += 5UL;
			num += (ushort)(this.regs.A << 8);
			this.regs.MW = (ushort)(((int)this.regs.A << 8) + (int)num + 1);
			this.regs.A = this.ReadPort(num);
		}

		private void OUT_NN_A(byte cmd)
		{
			this.Tact += 2UL;
			OnRDMEM readMemory = this.ReadMemory;
			Registers registers = this.regs;
			ushort pc;
			registers.PC = (pc = registers.PC) + 1;
			ushort num = (ushort)readMemory(pc, false);
			this.regs.MW = (ushort)((int)(num + 1 & 255) + ((int)this.regs.A << 8));
			this.Tact += 4UL;
			num += (ushort)(this.regs.A << 8);
			this.WritePort(num, this.regs.A);
			this.Tact += 1UL;
		}

		private void DI(byte cmd)
		{
			this.IFF1 = false;
			this.IFF2 = this.IFF1;
		}

		private void EI(byte cmd)
		{
			this.IFF1 = true;
			this.IFF2 = true;
			this.BlockINT = true;
		}

		private void LDSPHL(byte cmd)
		{
			this.regs.SP = this.regs.HL;
			this.Tact += 2UL;
		}

		private void EX_SP_HL(byte cmd)
		{
			ushort num = this.regs.SP;
			this.regs.MW = (ushort)this.ReadMemory(num, false);
			this.Tact += 3UL;
			this.WriteMemory(num, this.regs.L);
			this.Tact += 3UL;
			num += 1;
			Registers registers = this.regs;
			registers.MW += (ushort)((int)this.ReadMemory(num, false) * 256);
			this.Tact += 3UL;
			this.WriteMemory(num, this.regs.H);
			this.Tact += 3UL;
			this.regs.HL = this.regs.MW;
			this.Tact += 3UL;
		}

		private void JP_HL_(byte cmd)
		{
			this.regs.PC = this.regs.HL;
		}

		private void EXDEHL(byte cmd)
		{
			ushort hl = this.regs.HL;
			this.regs.HL = this.regs.DE;
			this.regs.DE = hl;
		}

		private void EXAFAF(byte cmd)
		{
			this.regs.EXAF();
		}

		private void EXX(byte cmd)
		{
			this.regs.EXX();
		}

		private void RLCA(byte cmd)
		{
			this.regs.F = (this.rlcaf[(int)this.regs.A] | (this.regs.F & 196));
			int num = (int)this.regs.A;
			num <<= 1;
			if ((num & 256) != 0)
			{
				num = ((num | 1) & 255);
			}
			this.regs.A = (byte)num;
		}

		private void RRCA(byte cmd)
		{
			this.regs.F = (this.rrcaf[(int)this.regs.A] | (this.regs.F & 196));
			int num = (int)this.regs.A;
			if ((num & 1) != 0)
			{
				num = (num >> 1 | 128);
			}
			else
			{
				num >>= 1;
			}
			this.regs.A = (byte)num;
		}

		private void RLA(byte cmd)
		{
			bool flag = (this.regs.F & 1) != 0;
			this.regs.F = (this.rlcaf[(int)this.regs.A] | (this.regs.F & 196));
			this.regs.A = (byte)((int)this.regs.A << 1 & 255);
			if (flag)
			{
				Registers registers = this.regs;
				registers.A |= 1;
			}
		}

		private void RRA(byte cmd)
		{
			bool flag = (this.regs.F & 1) != 0;
			this.regs.F = (this.rrcaf[(int)this.regs.A] | (this.regs.F & 196));
			this.regs.A = (byte)(this.regs.A >> 1);
			if (flag)
			{
				Registers registers = this.regs;
				registers.A |= 128;
			}
		}

		private void DAA(byte cmd)
		{
			this.regs.AF = Z80CPU.daatab[(int)this.regs.A + 256 * ((int)(this.regs.F & 3) + (this.regs.F >> 2 & 4))];
		}

		private void CPL(byte cmd)
		{
			Registers registers = this.regs;
			registers.A ^= byte.MaxValue;
			this.regs.F = ((this.regs.F & 215) | 18 | (this.regs.A & 40));
		}

		private void SCF(byte cmd)
		{
			this.regs.F = ((this.regs.F & 237) | (this.regs.A & 40) | 1);
		}

		private void CCF(byte cmd)
		{
			this.regs.F = (byte)(((int)(this.regs.F & 237) | ((int)this.regs.F << 4 & 16) | (int)(this.regs.A & 40)) ^ 1);
		}

		private void DJNZ(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 4UL;
			Registers registers = this.regs;
			registers.PC += 1;
			Registers registers2 = this.regs;
			if ((registers2.B -= 1) != 0)
			{
				this.regs.MW = (this.regs.PC = this.regs.PC + (ushort)((sbyte)num));
				this.Tact += 5UL;
			}
		}

		private void CALLNNNN(byte cmd)
		{
			this.regs.MW = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			Registers registers2 = this.regs;
			registers2.MW += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers3 = this.regs;
			registers3.PC += 1;
			this.Tact += 1UL;
			Registers registers4 = this.regs;
			registers4.SP -= 1;
			this.WriteMemory(this.regs.SP, (byte)(this.regs.PC >> 8));
			this.Tact += 3UL;
			Registers registers5 = this.regs;
			registers5.SP -= 1;
			this.WriteMemory(this.regs.SP, (byte)(this.regs.PC & 255));
			this.Tact += 3UL;
			this.regs.PC = this.regs.MW;
		}

		private void RET(byte cmd)
		{
			this.regs.MW = (ushort)this.ReadMemory(this.regs.SP, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.SP += 1;
			Registers registers2 = this.regs;
			registers2.MW += (ushort)((int)this.ReadMemory(this.regs.SP, false) * 256);
			this.Tact += 3UL;
			Registers registers3 = this.regs;
			registers3.SP += 1;
			this.regs.PC = this.regs.MW;
		}

		private void JPNNNN(byte cmd)
		{
			this.regs.MW = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			Registers registers2 = this.regs;
			registers2.MW += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			this.regs.PC = this.regs.MW;
		}

		private void JRNN(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			this.regs.MW = (this.regs.PC = this.regs.PC + (ushort)((sbyte)num));
			this.Tact += 5UL;
		}

		private void JRXNN(byte cmd)
		{
			int num = (cmd & 24) >> 3;
			ushort num2 = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			int num3 = (int)Z80CPU.conds[num >> 1];
			int num4 = (int)this.regs.AF & num3;
			if ((num & 1) != 0)
			{
				num4 ^= num3;
			}
			if (num4 == 0)
			{
				this.regs.MW = (this.regs.PC = this.regs.PC + (ushort)((sbyte)num2));
				this.Tact += 5UL;
			}
		}

		private void RETX(byte cmd)
		{
			int num = (cmd & 56) >> 3;
			this.Tact += 1UL;
			int num2 = (int)Z80CPU.conds[num >> 1];
			int num3 = (int)this.regs.AF & num2;
			if ((num & 1) != 0)
			{
				num3 ^= num2;
			}
			if (num3 == 0)
			{
				this.regs.MW = (ushort)this.ReadMemory(this.regs.SP, false);
				this.Tact += 3UL;
				Registers registers = this.regs;
				registers.SP += 1;
				Registers registers2 = this.regs;
				registers2.MW += (ushort)((int)this.ReadMemory(this.regs.SP, false) * 256);
				this.Tact += 3UL;
				Registers registers3 = this.regs;
				registers3.SP += 1;
				this.regs.PC = this.regs.MW;
			}
		}

		private void CALLXNNNN(byte cmd)
		{
			int num = (cmd & 56) >> 3;
			this.regs.MW = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			Registers registers2 = this.regs;
			registers2.MW += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers3 = this.regs;
			registers3.PC += 1;
			int num2 = (int)Z80CPU.conds[num >> 1];
			int num3 = (int)this.regs.AF & num2;
			if ((num & 1) != 0)
			{
				num3 ^= num2;
			}
			if (num3 == 0)
			{
				this.Tact += 1UL;
				Registers registers4 = this.regs;
				registers4.SP -= 1;
				this.WriteMemory(this.regs.SP, (byte)(this.regs.PC >> 8));
				this.Tact += 3UL;
				Registers registers5 = this.regs;
				registers5.SP -= 1;
				this.WriteMemory(this.regs.SP, (byte)this.regs.PC);
				this.Tact += 3UL;
				this.regs.PC = this.regs.MW;
			}
		}

		private void JPXNN(byte cmd)
		{
			int num = (cmd & 56) >> 3;
			this.regs.MW = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			Registers registers2 = this.regs;
			registers2.MW += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers3 = this.regs;
			registers3.PC += 1;
			int num2 = (int)Z80CPU.conds[num >> 1];
			int num3 = (int)this.regs.AF & num2;
			if ((num & 1) != 0)
			{
				num3 ^= num2;
			}
			if (num3 == 0)
			{
				this.regs.PC = this.regs.MW;
			}
		}

		private void RSTNN(byte cmd)
		{
			Registers registers = this.regs;
			registers.SP -= 1;
			this.Tact += 1UL;
			this.WriteMemory(this.regs.SP, (byte)(this.regs.PC >> 8));
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.SP -= 1;
			this.WriteMemory(this.regs.SP, (byte)this.regs.PC);
			this.Tact += 3UL;
			this.regs.MW = (ushort)(cmd & 56);
			this.regs.PC = this.regs.MW;
		}

		private void PUSHRR(byte cmd)
		{
			int num = (cmd & 48) >> 4;
			ushort num2;
			if (num == 3)
			{
				num2 = this.regs.AF;
			}
			else
			{
				num2 = this.regs.GetPair(num);
			}
			Registers registers = this.regs;
			registers.SP -= 1;
			this.Tact += 1UL;
			this.WriteMemory(this.regs.SP, (byte)(num2 >> 8));
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.SP -= 1;
			this.WriteMemory(this.regs.SP, (byte)(num2 & 255));
			this.Tact += 3UL;
		}

		private void POPRR(byte cmd)
		{
			int num = (cmd & 48) >> 4;
			ushort num2 = (ushort)this.ReadMemory(this.regs.SP, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.SP += 1;
			num2 |= (ushort)(this.ReadMemory(this.regs.SP, false) << 8);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.SP += 1;
			if (num == 3)
			{
				this.regs.AF = num2;
				return;
			}
			this.regs.SetPair(num, num2);
		}

		private void ALUAN(byte cmd)
		{
			int num = (cmd & 56) >> 3;
			byte src = this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			Z80CPU.alualg[num](src);
		}

		private void ALUAR(byte cmd)
		{
			int index = (int)(cmd & 7);
			int num = (cmd & 56) >> 3;
			Z80CPU.alualg[num](this.regs[index]);
		}

		private void ALUA_HL_(byte cmd)
		{
			int num = (cmd & 56) >> 3;
			byte src = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			Z80CPU.alualg[num](src);
		}

		private void ADDHLRR(byte cmd)
		{
			this.regs.MW = this.regs.HL + 1;
			this.regs.HL = this.ALU_ADDHLRR(this.regs.HL, this.regs.GetPair((cmd & 48) >> 4));
			this.Tact += 7UL;
		}

		private void LDA_RR_(byte cmd)
		{
			ushort pair = this.regs.GetPair((cmd & 48) >> 4);
			this.regs.MW = pair + 1;
			this.regs.A = this.ReadMemory(pair, false);
			this.Tact += 3UL;
		}

		private void LDA_NN_(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			this.regs.MW = num + 1;
			this.regs.A = this.ReadMemory(num, false);
			this.Tact += 3UL;
		}

		private void LDHL_NN_(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			this.regs.MW = num + 1;
			ushort num2 = (ushort)this.ReadMemory(num, false);
			this.Tact += 3UL;
			num2 += (ushort)((int)this.ReadMemory(this.regs.MW, false) * 256);
			this.Tact += 3UL;
			this.regs.HL = num2;
		}

		private void LD_RR_A(byte cmd)
		{
			this.WriteMemory(this.regs.GetPair((cmd & 48) >> 4), this.regs.A);
			this.regs.MH = this.regs.A;
			this.Tact += 3UL;
		}

		private void LD_NN_A(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			this.regs.MW = (ushort)((int)(num + 1 & 255) + ((int)this.regs.A << 8));
			this.WriteMemory(num, this.regs.A);
			this.regs.MH = this.regs.A;
			this.Tact += 3UL;
		}

		private void LD_NN_HL(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			this.regs.MW = num + 1;
			this.WriteMemory(num, this.regs.L);
			this.Tact += 3UL;
			this.WriteMemory(this.regs.MW, this.regs.H);
			this.Tact += 3UL;
		}

		private void LDRRNNNN(byte cmd)
		{
			int rr = (cmd & 48) >> 4;
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num |= (ushort)(this.ReadMemory(this.regs.PC, false) << 8);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			this.regs.SetPair(rr, num);
		}

		private void LDRNN(byte cmd)
		{
			int index = (cmd & 56) >> 3;
			this.regs[index] = this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
		}

		private void LD_HL_NN(byte cmd)
		{
			byte value = this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			this.WriteMemory(this.regs.HL, value);
			this.Tact += 3UL;
		}

		private void LDRdRs(byte cmd)
		{
			int index = (int)(cmd & 7);
			int index2 = (cmd & 56) >> 3;
			this.regs[index2] = this.regs[index];
		}

		private void LD_HL_R(byte cmd)
		{
			int index = (int)(cmd & 7);
			this.WriteMemory(this.regs.HL, this.regs[index]);
			this.Tact += 3UL;
		}

		private void LDR_HL_(byte cmd)
		{
			int index = (cmd & 56) >> 3;
			this.regs[index] = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
		}

		private void DECRR(byte cmd)
		{
			int rr = (cmd & 48) >> 4;
			this.regs.SetPair(rr, this.regs.GetPair(rr) - 1);
			this.Tact += 2UL;
		}

		private void INCRR(byte cmd)
		{
			int rr = (cmd & 48) >> 4;
			this.regs.SetPair(rr, this.regs.GetPair(rr) + 1);
			this.Tact += 2UL;
		}

		private void DECR(byte cmd)
		{
			int index = (cmd & 56) >> 3;
			this.regs[index] = this.ALU_DECR(this.regs[index]);
		}

		private void DEC_HL_(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			b = this.ALU_DECR(b);
			this.Tact += 1UL;
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void INCR(byte cmd)
		{
			int index = (cmd & 56) >> 3;
			this.regs[index] = this.ALU_INCR(this.regs[index]);
		}

		private void INC_HL_(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			b = this.ALU_INCR(b);
			this.Tact += 1UL;
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void HALT(byte cmd)
		{
			this.HALTED = true;
		}

		private void initExec()
		{
			this.opTABLE = new Z80CPU.XFXOPDO[]
			{
				null,
				new Z80CPU.XFXOPDO(this.LDRRNNNN),
				new Z80CPU.XFXOPDO(this.LD_RR_A),
				new Z80CPU.XFXOPDO(this.INCRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.RLCA),
				new Z80CPU.XFXOPDO(this.EXAFAF),
				new Z80CPU.XFXOPDO(this.ADDHLRR),
				new Z80CPU.XFXOPDO(this.LDA_RR_),
				new Z80CPU.XFXOPDO(this.DECRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.RRCA),
				new Z80CPU.XFXOPDO(this.DJNZ),
				new Z80CPU.XFXOPDO(this.LDRRNNNN),
				new Z80CPU.XFXOPDO(this.LD_RR_A),
				new Z80CPU.XFXOPDO(this.INCRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.RLA),
				new Z80CPU.XFXOPDO(this.JRNN),
				new Z80CPU.XFXOPDO(this.ADDHLRR),
				new Z80CPU.XFXOPDO(this.LDA_RR_),
				new Z80CPU.XFXOPDO(this.DECRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.RRA),
				new Z80CPU.XFXOPDO(this.JRXNN),
				new Z80CPU.XFXOPDO(this.LDRRNNNN),
				new Z80CPU.XFXOPDO(this.LD_NN_HL),
				new Z80CPU.XFXOPDO(this.INCRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.DAA),
				new Z80CPU.XFXOPDO(this.JRXNN),
				new Z80CPU.XFXOPDO(this.ADDHLRR),
				new Z80CPU.XFXOPDO(this.LDHL_NN_),
				new Z80CPU.XFXOPDO(this.DECRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.CPL),
				new Z80CPU.XFXOPDO(this.JRXNN),
				new Z80CPU.XFXOPDO(this.LDRRNNNN),
				new Z80CPU.XFXOPDO(this.LD_NN_A),
				new Z80CPU.XFXOPDO(this.INCRR),
				new Z80CPU.XFXOPDO(this.INC_HL_),
				new Z80CPU.XFXOPDO(this.DEC_HL_),
				new Z80CPU.XFXOPDO(this.LD_HL_NN),
				new Z80CPU.XFXOPDO(this.SCF),
				new Z80CPU.XFXOPDO(this.JRXNN),
				new Z80CPU.XFXOPDO(this.ADDHLRR),
				new Z80CPU.XFXOPDO(this.LDA_NN_),
				new Z80CPU.XFXOPDO(this.DECRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.CCF),
				null,
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDR_HL_),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				null,
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDR_HL_),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				null,
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDR_HL_),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				null,
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDR_HL_),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				null,
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDR_HL_),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				null,
				new Z80CPU.XFXOPDO(this.LDR_HL_),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LD_HL_R),
				new Z80CPU.XFXOPDO(this.LD_HL_R),
				new Z80CPU.XFXOPDO(this.LD_HL_R),
				new Z80CPU.XFXOPDO(this.LD_HL_R),
				new Z80CPU.XFXOPDO(this.LD_HL_R),
				new Z80CPU.XFXOPDO(this.LD_HL_R),
				new Z80CPU.XFXOPDO(this.HALT),
				new Z80CPU.XFXOPDO(this.LD_HL_R),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDR_HL_),
				null,
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUA_HL_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUA_HL_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUA_HL_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUA_HL_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUA_HL_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUA_HL_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUA_HL_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUA_HL_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.POPRR),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.JPNNNN),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				new Z80CPU.XFXOPDO(this.PUSHRR),
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.RET),
				new Z80CPU.XFXOPDO(this.JPXNN),
				null,
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				new Z80CPU.XFXOPDO(this.CALLNNNN),
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.POPRR),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.OUT_NN_A),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				new Z80CPU.XFXOPDO(this.PUSHRR),
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.EXX),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.INA_NN_),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				null,
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.POPRR),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.EX_SP_HL),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				new Z80CPU.XFXOPDO(this.PUSHRR),
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.JP_HL_),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.EXDEHL),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				null,
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.POPRR),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.DI),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				new Z80CPU.XFXOPDO(this.PUSHRR),
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.LDSPHL),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.EI),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				null,
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN)
			};
		}

		private void FX_LDSPHL(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				this.regs.SP = this.regs.IX;
			}
			else
			{
				this.regs.SP = this.regs.IY;
			}
			this.Tact += 2UL;
		}

		private void FX_EX_SP_HL(byte cmd)
		{
			ushort num = this.regs.SP;
			ushort num2 = (ushort)this.ReadMemory(num, false);
			this.Tact += 3UL;
			if (this.FX == OPFX.IX)
			{
				this.WriteMemory(num, this.regs.XL);
			}
			else
			{
				this.WriteMemory(num, this.regs.YL);
			}
			this.Tact += 3UL;
			num2 += (ushort)((int)this.ReadMemory(num += 1, false) * 256);
			this.Tact += 3UL;
			if (this.FX == OPFX.IX)
			{
				this.WriteMemory(num, this.regs.XH);
				this.regs.IX = num2;
			}
			else
			{
				this.WriteMemory(num, this.regs.YH);
				this.regs.IY = num2;
			}
			this.regs.MW = num2;
			this.Tact += 6UL;
		}

		private void FX_JP_HL_(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				this.regs.PC = this.regs.IX;
				return;
			}
			this.regs.PC = this.regs.IY;
		}

		private void FX_PUSHIX(byte cmd)
		{
			ushort num;
			if (this.FX == OPFX.IX)
			{
				num = this.regs.IX;
			}
			else
			{
				num = this.regs.IY;
			}
			Registers registers = this.regs;
			registers.SP -= 1;
			this.Tact += 1UL;
			this.WriteMemory(this.regs.SP, (byte)(num >> 8));
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.SP -= 1;
			this.WriteMemory(this.regs.SP, (byte)(num & 255));
			this.Tact += 3UL;
		}

		private void FX_POPIX(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.SP, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.SP += 1;
			num |= (ushort)(this.ReadMemory(this.regs.SP, false) << 8);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.SP += 1;
			if (this.FX == OPFX.IX)
			{
				this.regs.IX = num;
				return;
			}
			this.regs.IY = num;
		}

		private void FX_ALUAXH(byte cmd)
		{
			byte src;
			if (this.FX == OPFX.IX)
			{
				src = (byte)(this.regs.IX >> 8);
			}
			else
			{
				src = (byte)(this.regs.IY >> 8);
			}
			Z80CPU.alualg[(cmd & 56) >> 3](src);
		}

		private void FX_ALUAXL(byte cmd)
		{
			byte src;
			if (this.FX == OPFX.IX)
			{
				src = (byte)(this.regs.IX & 255);
			}
			else
			{
				src = (byte)(this.regs.IY & 255);
			}
			Z80CPU.alualg[(cmd & 56) >> 3](src);
		}

		private void FX_ALUA_IX_(byte cmd)
		{
			int num = (cmd & 56) >> 3;
			ushort num2;
			if (this.FX == OPFX.IX)
			{
				num2 = this.regs.IX;
			}
			else
			{
				num2 = this.regs.IY;
			}
			ushort num3 = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num2 += (ushort)((sbyte)num3);
			this.Tact += 5UL;
			byte src = this.ReadMemory(num2, false);
			this.Tact += 3UL;
			Z80CPU.alualg[num](src);
		}

		private void FX_ADDIXRR(byte cmd)
		{
			this.regs.MW = this.regs.IX + 1;
			ushort rde;
			switch ((cmd & 48) >> 4)
			{
			case 0:
				rde = this.regs.BC;
				break;
			case 1:
				rde = this.regs.DE;
				break;
			case 2:
				if (this.FX == OPFX.IX)
				{
					rde = this.regs.IX;
				}
				else
				{
					rde = this.regs.IY;
				}
				break;
			case 3:
				rde = this.regs.SP;
				break;
			default:
				throw new Exception("Error decode reg in FX_ADDIXRR!");
			}
			if (this.FX == OPFX.IX)
			{
				this.regs.IX = this.ALU_ADDHLRR(this.regs.IX, rde);
			}
			else
			{
				this.regs.IY = this.ALU_ADDHLRR(this.regs.IY, rde);
			}
			this.Tact += 7UL;
		}

		private void FX_DECIX(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				Registers registers = this.regs;
				registers.IX -= 1;
			}
			else
			{
				Registers registers2 = this.regs;
				registers2.IY -= 1;
			}
			this.Tact += 2UL;
		}

		private void FX_INCIX(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				Registers registers = this.regs;
				registers.IX += 1;
			}
			else
			{
				Registers registers2 = this.regs;
				registers2.IY += 1;
			}
			this.Tact += 2UL;
		}

		private void FX_LDIX_N_(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			this.regs.MW = num + 1;
			ushort num2 = (ushort)this.ReadMemory(num, false);
			this.Tact += 3UL;
			num2 += (ushort)((int)this.ReadMemory(this.regs.MW, false) * 256);
			this.Tact += 3UL;
			if (this.FX == OPFX.IX)
			{
				this.regs.IX = num2;
				return;
			}
			this.regs.IY = num2;
		}

		private void FX_LD_NN_IX(byte cmd)
		{
			ushort num;
			if (this.FX == OPFX.IX)
			{
				num = this.regs.IX;
			}
			else
			{
				num = this.regs.IY;
			}
			ushort num2 = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num2 += (ushort)((int)this.ReadMemory(this.regs.PC, false) * 256);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			this.regs.MW = num2 + 1;
			this.WriteMemory(num2, (byte)num);
			this.Tact += 3UL;
			this.WriteMemory(this.regs.MW, (byte)(num >> 8));
			this.Tact += 3UL;
		}

		private void FX_LDIXNNNN(byte cmd)
		{
			ushort num = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num |= (ushort)(this.ReadMemory(this.regs.PC, false) << 8);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			if (this.FX == OPFX.IX)
			{
				this.regs.IX = num;
				return;
			}
			this.regs.IY = num;
		}

		private void FX_DEC_IX_(byte cmd)
		{
			ushort num;
			if (this.FX == OPFX.IX)
			{
				num = this.regs.IX;
			}
			else
			{
				num = this.regs.IY;
			}
			ushort num2 = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((sbyte)num2);
			this.Tact += 5UL;
			byte b = this.ReadMemory(num, false);
			this.Tact += 3UL;
			b = this.ALU_DECR(b);
			this.Tact += 1UL;
			this.WriteMemory(num, b);
			this.Tact += 3UL;
		}

		private void FX_INC_IX_(byte cmd)
		{
			ushort num;
			if (this.FX == OPFX.IX)
			{
				num = this.regs.IX;
			}
			else
			{
				num = this.regs.IY;
			}
			ushort num2 = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((sbyte)num2);
			this.Tact += 5UL;
			byte b = this.ReadMemory(num, false);
			this.Tact += 3UL;
			b = this.ALU_INCR(b);
			this.Tact += 1UL;
			this.WriteMemory(num, b);
			this.Tact += 3UL;
		}

		private void FX_LD_IX_NN(byte cmd)
		{
			ushort num;
			if (this.FX == OPFX.IX)
			{
				num = this.regs.IX;
			}
			else
			{
				num = this.regs.IY;
			}
			ushort num2 = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((sbyte)num2);
			this.Tact += 2UL;
			byte value = this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers2 = this.regs;
			registers2.PC += 1;
			this.WriteMemory(num, value);
			this.Tact += 3UL;
		}

		private void FX_LD_IX_R(byte cmd)
		{
			int index = (int)(cmd & 7);
			ushort num;
			if (this.FX == OPFX.IX)
			{
				num = this.regs.IX;
			}
			else
			{
				num = this.regs.IY;
			}
			ushort num2 = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((sbyte)num2);
			this.Tact += 5UL;
			this.WriteMemory(num, this.regs[index]);
			this.Tact += 3UL;
		}

		private void FX_LDR_IX_(byte cmd)
		{
			int index = (cmd & 56) >> 3;
			ushort num;
			if (this.FX == OPFX.IX)
			{
				num = this.regs.IX;
			}
			else
			{
				num = this.regs.IY;
			}
			ushort num2 = (ushort)this.ReadMemory(this.regs.PC, false);
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
			num += (ushort)((sbyte)num2);
			this.Tact += 5UL;
			this.regs[index] = this.ReadMemory(num, false);
			this.Tact += 3UL;
		}

		private void FX_LDHL(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				this.regs.XH = this.regs.XL;
				return;
			}
			this.regs.YH = this.regs.YL;
		}

		private void FX_LDLH(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				this.regs.XL = this.regs.XH;
				return;
			}
			this.regs.YL = this.regs.YH;
		}

		private void FX_LDRL(byte cmd)
		{
			int index = (cmd & 56) >> 3;
			if (this.FX == OPFX.IX)
			{
				this.regs[index] = this.regs.XL;
				return;
			}
			this.regs[index] = this.regs.YL;
		}

		private void FX_LDRH(byte cmd)
		{
			int index = (cmd & 56) >> 3;
			if (this.FX == OPFX.IX)
			{
				this.regs[index] = this.regs.XH;
				return;
			}
			this.regs[index] = this.regs.YH;
		}

		private void FX_LDLR(byte cmd)
		{
			int index = (int)(cmd & 7);
			if (this.FX == OPFX.IX)
			{
				this.regs.XL = this.regs[index];
				return;
			}
			this.regs.YL = this.regs[index];
		}

		private void FX_LDHR(byte cmd)
		{
			int index = (int)(cmd & 7);
			if (this.FX == OPFX.IX)
			{
				this.regs.XH = this.regs[index];
				return;
			}
			this.regs.YH = this.regs[index];
		}

		private void FX_LDLNN(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				this.regs.XL = this.ReadMemory(this.regs.PC, false);
			}
			else
			{
				this.regs.YL = this.ReadMemory(this.regs.PC, false);
			}
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
		}

		private void FX_LDHNN(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				this.regs.XH = this.ReadMemory(this.regs.PC, false);
			}
			else
			{
				this.regs.YH = this.ReadMemory(this.regs.PC, false);
			}
			this.Tact += 3UL;
			Registers registers = this.regs;
			registers.PC += 1;
		}

		private void FX_INCL(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				this.regs.XL = this.ALU_INCR(this.regs.XL);
				return;
			}
			this.regs.YL = this.ALU_INCR(this.regs.YL);
		}

		private void FX_INCH(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				this.regs.XH = this.ALU_INCR(this.regs.XH);
				return;
			}
			this.regs.YH = this.ALU_INCR(this.regs.YH);
		}

		private void FX_DECL(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				this.regs.XL = this.ALU_DECR(this.regs.XL);
				return;
			}
			this.regs.YL = this.ALU_DECR(this.regs.YL);
		}

		private void FX_DECH(byte cmd)
		{
			if (this.FX == OPFX.IX)
			{
				this.regs.XH = this.ALU_DECR(this.regs.XH);
				return;
			}
			this.regs.YH = this.ALU_DECR(this.regs.YH);
		}

		private void initExecFX()
		{
			this.fxopTABLE = new Z80CPU.XFXOPDO[]
			{
				null,
				new Z80CPU.XFXOPDO(this.LDRRNNNN),
				new Z80CPU.XFXOPDO(this.LD_RR_A),
				new Z80CPU.XFXOPDO(this.INCRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.RLCA),
				new Z80CPU.XFXOPDO(this.EXAFAF),
				new Z80CPU.XFXOPDO(this.FX_ADDIXRR),
				new Z80CPU.XFXOPDO(this.LDA_RR_),
				new Z80CPU.XFXOPDO(this.DECRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.RRCA),
				new Z80CPU.XFXOPDO(this.DJNZ),
				new Z80CPU.XFXOPDO(this.LDRRNNNN),
				new Z80CPU.XFXOPDO(this.LD_RR_A),
				new Z80CPU.XFXOPDO(this.INCRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.RLA),
				new Z80CPU.XFXOPDO(this.JRNN),
				new Z80CPU.XFXOPDO(this.FX_ADDIXRR),
				new Z80CPU.XFXOPDO(this.LDA_RR_),
				new Z80CPU.XFXOPDO(this.DECRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.RRA),
				new Z80CPU.XFXOPDO(this.JRXNN),
				new Z80CPU.XFXOPDO(this.FX_LDIXNNNN),
				new Z80CPU.XFXOPDO(this.FX_LD_NN_IX),
				new Z80CPU.XFXOPDO(this.FX_INCIX),
				new Z80CPU.XFXOPDO(this.FX_INCH),
				new Z80CPU.XFXOPDO(this.FX_DECH),
				new Z80CPU.XFXOPDO(this.FX_LDHNN),
				new Z80CPU.XFXOPDO(this.DAA),
				new Z80CPU.XFXOPDO(this.JRXNN),
				new Z80CPU.XFXOPDO(this.FX_ADDIXRR),
				new Z80CPU.XFXOPDO(this.FX_LDIX_N_),
				new Z80CPU.XFXOPDO(this.FX_DECIX),
				new Z80CPU.XFXOPDO(this.FX_INCL),
				new Z80CPU.XFXOPDO(this.FX_DECL),
				new Z80CPU.XFXOPDO(this.FX_LDLNN),
				new Z80CPU.XFXOPDO(this.CPL),
				new Z80CPU.XFXOPDO(this.JRXNN),
				new Z80CPU.XFXOPDO(this.LDRRNNNN),
				new Z80CPU.XFXOPDO(this.LD_NN_A),
				new Z80CPU.XFXOPDO(this.INCRR),
				new Z80CPU.XFXOPDO(this.FX_INC_IX_),
				new Z80CPU.XFXOPDO(this.FX_DEC_IX_),
				new Z80CPU.XFXOPDO(this.FX_LD_IX_NN),
				new Z80CPU.XFXOPDO(this.SCF),
				new Z80CPU.XFXOPDO(this.JRXNN),
				new Z80CPU.XFXOPDO(this.FX_ADDIXRR),
				new Z80CPU.XFXOPDO(this.LDA_NN_),
				new Z80CPU.XFXOPDO(this.DECRR),
				new Z80CPU.XFXOPDO(this.INCR),
				new Z80CPU.XFXOPDO(this.DECR),
				new Z80CPU.XFXOPDO(this.LDRNN),
				new Z80CPU.XFXOPDO(this.CCF),
				null,
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.FX_LDRH),
				new Z80CPU.XFXOPDO(this.FX_LDRL),
				new Z80CPU.XFXOPDO(this.FX_LDR_IX_),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				null,
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.FX_LDRH),
				new Z80CPU.XFXOPDO(this.FX_LDRL),
				new Z80CPU.XFXOPDO(this.FX_LDR_IX_),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				null,
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.FX_LDRH),
				new Z80CPU.XFXOPDO(this.FX_LDRL),
				new Z80CPU.XFXOPDO(this.FX_LDR_IX_),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				null,
				new Z80CPU.XFXOPDO(this.FX_LDRH),
				new Z80CPU.XFXOPDO(this.FX_LDRL),
				new Z80CPU.XFXOPDO(this.FX_LDR_IX_),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.FX_LDHR),
				new Z80CPU.XFXOPDO(this.FX_LDHR),
				new Z80CPU.XFXOPDO(this.FX_LDHR),
				new Z80CPU.XFXOPDO(this.FX_LDHR),
				null,
				new Z80CPU.XFXOPDO(this.FX_LDHL),
				new Z80CPU.XFXOPDO(this.FX_LDR_IX_),
				new Z80CPU.XFXOPDO(this.FX_LDHR),
				new Z80CPU.XFXOPDO(this.FX_LDLR),
				new Z80CPU.XFXOPDO(this.FX_LDLR),
				new Z80CPU.XFXOPDO(this.FX_LDLR),
				new Z80CPU.XFXOPDO(this.FX_LDLR),
				new Z80CPU.XFXOPDO(this.FX_LDLH),
				null,
				new Z80CPU.XFXOPDO(this.FX_LDR_IX_),
				new Z80CPU.XFXOPDO(this.FX_LDLR),
				new Z80CPU.XFXOPDO(this.FX_LD_IX_R),
				new Z80CPU.XFXOPDO(this.FX_LD_IX_R),
				new Z80CPU.XFXOPDO(this.FX_LD_IX_R),
				new Z80CPU.XFXOPDO(this.FX_LD_IX_R),
				new Z80CPU.XFXOPDO(this.FX_LD_IX_R),
				new Z80CPU.XFXOPDO(this.FX_LD_IX_R),
				new Z80CPU.XFXOPDO(this.HALT),
				new Z80CPU.XFXOPDO(this.FX_LD_IX_R),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.LDRdRs),
				new Z80CPU.XFXOPDO(this.FX_LDRH),
				new Z80CPU.XFXOPDO(this.FX_LDRL),
				new Z80CPU.XFXOPDO(this.FX_LDR_IX_),
				null,
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.FX_ALUAXH),
				new Z80CPU.XFXOPDO(this.FX_ALUAXL),
				new Z80CPU.XFXOPDO(this.FX_ALUA_IX_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.FX_ALUAXH),
				new Z80CPU.XFXOPDO(this.FX_ALUAXL),
				new Z80CPU.XFXOPDO(this.FX_ALUA_IX_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.FX_ALUAXH),
				new Z80CPU.XFXOPDO(this.FX_ALUAXL),
				new Z80CPU.XFXOPDO(this.FX_ALUA_IX_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.FX_ALUAXH),
				new Z80CPU.XFXOPDO(this.FX_ALUAXL),
				new Z80CPU.XFXOPDO(this.FX_ALUA_IX_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.FX_ALUAXH),
				new Z80CPU.XFXOPDO(this.FX_ALUAXL),
				new Z80CPU.XFXOPDO(this.FX_ALUA_IX_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.FX_ALUAXH),
				new Z80CPU.XFXOPDO(this.FX_ALUAXL),
				new Z80CPU.XFXOPDO(this.FX_ALUA_IX_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.FX_ALUAXH),
				new Z80CPU.XFXOPDO(this.FX_ALUAXL),
				new Z80CPU.XFXOPDO(this.FX_ALUA_IX_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.FX_ALUAXH),
				new Z80CPU.XFXOPDO(this.FX_ALUAXL),
				new Z80CPU.XFXOPDO(this.FX_ALUA_IX_),
				new Z80CPU.XFXOPDO(this.ALUAR),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.POPRR),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.JPNNNN),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				new Z80CPU.XFXOPDO(this.PUSHRR),
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.RET),
				new Z80CPU.XFXOPDO(this.JPXNN),
				null,
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				new Z80CPU.XFXOPDO(this.CALLNNNN),
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.POPRR),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.OUT_NN_A),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				new Z80CPU.XFXOPDO(this.PUSHRR),
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.EXX),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.INA_NN_),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				null,
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.FX_POPIX),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.FX_EX_SP_HL),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				new Z80CPU.XFXOPDO(this.FX_PUSHIX),
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.FX_JP_HL_),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.EXDEHL),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				null,
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.POPRR),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.DI),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				new Z80CPU.XFXOPDO(this.PUSHRR),
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN),
				new Z80CPU.XFXOPDO(this.RETX),
				new Z80CPU.XFXOPDO(this.FX_LDSPHL),
				new Z80CPU.XFXOPDO(this.JPXNN),
				new Z80CPU.XFXOPDO(this.EI),
				new Z80CPU.XFXOPDO(this.CALLXNNNN),
				null,
				new Z80CPU.XFXOPDO(this.ALUAN),
				new Z80CPU.XFXOPDO(this.RSTNN)
			};
		}

		private void CB_RLC(byte cmd)
		{
			this.regs[(int)(cmd & 7)] = this.ALU_RLC((int)this.regs[(int)(cmd & 7)]);
		}

		private void CB_RRC(byte cmd)
		{
			this.regs[(int)(cmd & 7)] = this.ALU_RRC((int)this.regs[(int)(cmd & 7)]);
		}

		private void CB_RL(byte cmd)
		{
			this.regs[(int)(cmd & 7)] = this.ALU_RL((int)this.regs[(int)(cmd & 7)]);
		}

		private void CB_RR(byte cmd)
		{
			this.regs[(int)(cmd & 7)] = this.ALU_RR((int)this.regs[(int)(cmd & 7)]);
		}

		private void CB_SLA(byte cmd)
		{
			this.regs[(int)(cmd & 7)] = this.ALU_SLA((int)this.regs[(int)(cmd & 7)]);
		}

		private void CB_SRA(byte cmd)
		{
			this.regs[(int)(cmd & 7)] = this.ALU_SRA((int)this.regs[(int)(cmd & 7)]);
		}

		private void CB_SLL(byte cmd)
		{
			this.regs[(int)(cmd & 7)] = this.ALU_SLL((int)this.regs[(int)(cmd & 7)]);
		}

		private void CB_SRL(byte cmd)
		{
			this.regs[(int)(cmd & 7)] = this.ALU_SRL((int)this.regs[(int)(cmd & 7)]);
		}

		private void CB_BIT(byte cmd)
		{
			this.ALU_BIT(this.regs[(int)(cmd & 7)], (cmd & 56) >> 3);
		}

		private void CB_RES(byte cmd)
		{
			Registers registers;
			int index;
			(registers = this.regs)[index = (int)(cmd & 7)] = (registers[index] & (byte)(~(byte)(1 << ((cmd & 56) >> 3))));
		}

		private void CB_SET(byte cmd)
		{
			Registers registers;
			int index;
			(registers = this.regs)[index = (int)(cmd & 7)] = (registers[index] | (byte)(1 << ((cmd & 56) >> 3)));
		}

		private void CB_RLCHL(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RLC((int)b);
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void CB_RRCHL(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RRC((int)b);
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void CB_RLHL(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RL((int)b);
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void CB_RRHL(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RR((int)b);
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void CB_SLAHL(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SLA((int)b);
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void CB_SRAHL(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SRA((int)b);
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void CB_SLLHL(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SLL((int)b);
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void CB_SRLHL(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SRL((int)b);
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void CB_BITHL(byte cmd)
		{
			byte src = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			this.ALU_BITMEM(src, (cmd & 56) >> 3);
		}

		private void CB_RESHL(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b &= (byte)(~(byte)(1 << ((cmd & 56) >> 3)));
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void CB_SETHL(byte cmd)
		{
			byte b = this.ReadMemory(this.regs.HL, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b |= (byte)(1 << ((cmd & 56) >> 3));
			this.WriteMemory(this.regs.HL, b);
			this.Tact += 3UL;
		}

		private void FXCB_RLC(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RLC((int)b);
			this.WriteMemory(adr, b);
			this.regs[(int)(cmd & 7)] = b;
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_RRC(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RRC((int)b);
			this.WriteMemory(adr, b);
			this.regs[(int)(cmd & 7)] = b;
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_RL(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RL((int)b);
			this.WriteMemory(adr, b);
			this.regs[(int)(cmd & 7)] = b;
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_RR(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RR((int)b);
			this.WriteMemory(adr, b);
			this.regs[(int)(cmd & 7)] = b;
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_SLA(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SLA((int)b);
			this.WriteMemory(adr, b);
			this.regs[(int)(cmd & 7)] = b;
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_SRA(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SRA((int)b);
			this.WriteMemory(adr, b);
			this.regs[(int)(cmd & 7)] = b;
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_SLL(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SLL((int)b);
			this.WriteMemory(adr, b);
			this.regs[(int)(cmd & 7)] = b;
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_SRL(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SRL((int)b);
			this.WriteMemory(adr, b);
			this.regs[(int)(cmd & 7)] = b;
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_BIT(byte cmd, ushort adr)
		{
			this.FXCB_BITIX(cmd, adr);
		}

		private void FXCB_RES(byte cmd, ushort adr)
		{
			this.FXCB_RESIX(cmd, adr);
		}

		private void FXCB_SET(byte cmd, ushort adr)
		{
			this.FXCB_SETIX(cmd, adr);
		}

		private void FXCB_RLCIX(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RLC((int)b);
			this.WriteMemory(adr, b);
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_RRCIX(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RRC((int)b);
			this.WriteMemory(adr, b);
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_RLIX(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RL((int)b);
			this.WriteMemory(adr, b);
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_RRIX(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_RR((int)b);
			this.WriteMemory(adr, b);
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_SLAIX(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SLA((int)b);
			this.WriteMemory(adr, b);
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_SRAIX(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SRA((int)b);
			this.WriteMemory(adr, b);
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_SLLIX(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SLL((int)b);
			this.WriteMemory(adr, b);
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_SRLIX(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b = this.ALU_SRL((int)b);
			this.WriteMemory(adr, b);
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_BITIX(byte cmd, ushort adr)
		{
			byte src = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			this.ALU_BITMEM(src, (cmd & 56) >> 3);
			this.RegenMemory();
		}

		private void FXCB_RESIX(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b &= (byte)(~(byte)(1 << ((cmd & 56) >> 3)));
			this.WriteMemory(adr, b);
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void FXCB_SETIX(byte cmd, ushort adr)
		{
			byte b = this.ReadMemory(adr, false);
			this.Tact += 3UL;
			this.Tact += 1UL;
			b |= (byte)(1 << ((cmd & 56) >> 3));
			this.WriteMemory(adr, b);
			this.Tact += 3UL;
			this.Tact += 1UL;
		}

		private void initExecCB()
		{
			this.cbopTABLE = new Z80CPU.XFXOPDO[]
			{
				new Z80CPU.XFXOPDO(this.CB_RLC),
				new Z80CPU.XFXOPDO(this.CB_RLC),
				new Z80CPU.XFXOPDO(this.CB_RLC),
				new Z80CPU.XFXOPDO(this.CB_RLC),
				new Z80CPU.XFXOPDO(this.CB_RLC),
				new Z80CPU.XFXOPDO(this.CB_RLC),
				new Z80CPU.XFXOPDO(this.CB_RLCHL),
				new Z80CPU.XFXOPDO(this.CB_RLC),
				new Z80CPU.XFXOPDO(this.CB_RRC),
				new Z80CPU.XFXOPDO(this.CB_RRC),
				new Z80CPU.XFXOPDO(this.CB_RRC),
				new Z80CPU.XFXOPDO(this.CB_RRC),
				new Z80CPU.XFXOPDO(this.CB_RRC),
				new Z80CPU.XFXOPDO(this.CB_RRC),
				new Z80CPU.XFXOPDO(this.CB_RRCHL),
				new Z80CPU.XFXOPDO(this.CB_RRC),
				new Z80CPU.XFXOPDO(this.CB_RL),
				new Z80CPU.XFXOPDO(this.CB_RL),
				new Z80CPU.XFXOPDO(this.CB_RL),
				new Z80CPU.XFXOPDO(this.CB_RL),
				new Z80CPU.XFXOPDO(this.CB_RL),
				new Z80CPU.XFXOPDO(this.CB_RL),
				new Z80CPU.XFXOPDO(this.CB_RLHL),
				new Z80CPU.XFXOPDO(this.CB_RL),
				new Z80CPU.XFXOPDO(this.CB_RR),
				new Z80CPU.XFXOPDO(this.CB_RR),
				new Z80CPU.XFXOPDO(this.CB_RR),
				new Z80CPU.XFXOPDO(this.CB_RR),
				new Z80CPU.XFXOPDO(this.CB_RR),
				new Z80CPU.XFXOPDO(this.CB_RR),
				new Z80CPU.XFXOPDO(this.CB_RRHL),
				new Z80CPU.XFXOPDO(this.CB_RR),
				new Z80CPU.XFXOPDO(this.CB_SLA),
				new Z80CPU.XFXOPDO(this.CB_SLA),
				new Z80CPU.XFXOPDO(this.CB_SLA),
				new Z80CPU.XFXOPDO(this.CB_SLA),
				new Z80CPU.XFXOPDO(this.CB_SLA),
				new Z80CPU.XFXOPDO(this.CB_SLA),
				new Z80CPU.XFXOPDO(this.CB_SLAHL),
				new Z80CPU.XFXOPDO(this.CB_SLA),
				new Z80CPU.XFXOPDO(this.CB_SRA),
				new Z80CPU.XFXOPDO(this.CB_SRA),
				new Z80CPU.XFXOPDO(this.CB_SRA),
				new Z80CPU.XFXOPDO(this.CB_SRA),
				new Z80CPU.XFXOPDO(this.CB_SRA),
				new Z80CPU.XFXOPDO(this.CB_SRA),
				new Z80CPU.XFXOPDO(this.CB_SRAHL),
				new Z80CPU.XFXOPDO(this.CB_SRA),
				new Z80CPU.XFXOPDO(this.CB_SLL),
				new Z80CPU.XFXOPDO(this.CB_SLL),
				new Z80CPU.XFXOPDO(this.CB_SLL),
				new Z80CPU.XFXOPDO(this.CB_SLL),
				new Z80CPU.XFXOPDO(this.CB_SLL),
				new Z80CPU.XFXOPDO(this.CB_SLL),
				new Z80CPU.XFXOPDO(this.CB_SLLHL),
				new Z80CPU.XFXOPDO(this.CB_SLL),
				new Z80CPU.XFXOPDO(this.CB_SRL),
				new Z80CPU.XFXOPDO(this.CB_SRL),
				new Z80CPU.XFXOPDO(this.CB_SRL),
				new Z80CPU.XFXOPDO(this.CB_SRL),
				new Z80CPU.XFXOPDO(this.CB_SRL),
				new Z80CPU.XFXOPDO(this.CB_SRL),
				new Z80CPU.XFXOPDO(this.CB_SRLHL),
				new Z80CPU.XFXOPDO(this.CB_SRL),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BITHL),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BITHL),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BITHL),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BITHL),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BITHL),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BITHL),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BITHL),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_BITHL),
				new Z80CPU.XFXOPDO(this.CB_BIT),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RESHL),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RESHL),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RESHL),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RESHL),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RESHL),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RESHL),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RESHL),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_RESHL),
				new Z80CPU.XFXOPDO(this.CB_RES),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SETHL),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SETHL),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SETHL),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SETHL),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SETHL),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SETHL),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SETHL),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SET),
				new Z80CPU.XFXOPDO(this.CB_SETHL),
				new Z80CPU.XFXOPDO(this.CB_SET)
			};
			this.fxcbopTABLE = new Z80CPU.FXCBOPDO[]
			{
				new Z80CPU.FXCBOPDO(this.FXCB_RLC),
				new Z80CPU.FXCBOPDO(this.FXCB_RLC),
				new Z80CPU.FXCBOPDO(this.FXCB_RLC),
				new Z80CPU.FXCBOPDO(this.FXCB_RLC),
				new Z80CPU.FXCBOPDO(this.FXCB_RLC),
				new Z80CPU.FXCBOPDO(this.FXCB_RLC),
				new Z80CPU.FXCBOPDO(this.FXCB_RLCIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RLC),
				new Z80CPU.FXCBOPDO(this.FXCB_RRC),
				new Z80CPU.FXCBOPDO(this.FXCB_RRC),
				new Z80CPU.FXCBOPDO(this.FXCB_RRC),
				new Z80CPU.FXCBOPDO(this.FXCB_RRC),
				new Z80CPU.FXCBOPDO(this.FXCB_RRC),
				new Z80CPU.FXCBOPDO(this.FXCB_RRC),
				new Z80CPU.FXCBOPDO(this.FXCB_RRCIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RRC),
				new Z80CPU.FXCBOPDO(this.FXCB_RL),
				new Z80CPU.FXCBOPDO(this.FXCB_RL),
				new Z80CPU.FXCBOPDO(this.FXCB_RL),
				new Z80CPU.FXCBOPDO(this.FXCB_RL),
				new Z80CPU.FXCBOPDO(this.FXCB_RL),
				new Z80CPU.FXCBOPDO(this.FXCB_RL),
				new Z80CPU.FXCBOPDO(this.FXCB_RLIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RL),
				new Z80CPU.FXCBOPDO(this.FXCB_RR),
				new Z80CPU.FXCBOPDO(this.FXCB_RR),
				new Z80CPU.FXCBOPDO(this.FXCB_RR),
				new Z80CPU.FXCBOPDO(this.FXCB_RR),
				new Z80CPU.FXCBOPDO(this.FXCB_RR),
				new Z80CPU.FXCBOPDO(this.FXCB_RR),
				new Z80CPU.FXCBOPDO(this.FXCB_RRIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RR),
				new Z80CPU.FXCBOPDO(this.FXCB_SLA),
				new Z80CPU.FXCBOPDO(this.FXCB_SLA),
				new Z80CPU.FXCBOPDO(this.FXCB_SLA),
				new Z80CPU.FXCBOPDO(this.FXCB_SLA),
				new Z80CPU.FXCBOPDO(this.FXCB_SLA),
				new Z80CPU.FXCBOPDO(this.FXCB_SLA),
				new Z80CPU.FXCBOPDO(this.FXCB_SLAIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SLA),
				new Z80CPU.FXCBOPDO(this.FXCB_SRA),
				new Z80CPU.FXCBOPDO(this.FXCB_SRA),
				new Z80CPU.FXCBOPDO(this.FXCB_SRA),
				new Z80CPU.FXCBOPDO(this.FXCB_SRA),
				new Z80CPU.FXCBOPDO(this.FXCB_SRA),
				new Z80CPU.FXCBOPDO(this.FXCB_SRA),
				new Z80CPU.FXCBOPDO(this.FXCB_SRAIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SRA),
				new Z80CPU.FXCBOPDO(this.FXCB_SLL),
				new Z80CPU.FXCBOPDO(this.FXCB_SLL),
				new Z80CPU.FXCBOPDO(this.FXCB_SLL),
				new Z80CPU.FXCBOPDO(this.FXCB_SLL),
				new Z80CPU.FXCBOPDO(this.FXCB_SLL),
				new Z80CPU.FXCBOPDO(this.FXCB_SLL),
				new Z80CPU.FXCBOPDO(this.FXCB_SLLIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SLL),
				new Z80CPU.FXCBOPDO(this.FXCB_SRL),
				new Z80CPU.FXCBOPDO(this.FXCB_SRL),
				new Z80CPU.FXCBOPDO(this.FXCB_SRL),
				new Z80CPU.FXCBOPDO(this.FXCB_SRL),
				new Z80CPU.FXCBOPDO(this.FXCB_SRL),
				new Z80CPU.FXCBOPDO(this.FXCB_SRL),
				new Z80CPU.FXCBOPDO(this.FXCB_SRLIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SRL),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BITIX),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BITIX),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BITIX),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BITIX),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BITIX),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BITIX),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BITIX),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_BITIX),
				new Z80CPU.FXCBOPDO(this.FXCB_BIT),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RESIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RESIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RESIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RESIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RESIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RESIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RESIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_RESIX),
				new Z80CPU.FXCBOPDO(this.FXCB_RES),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SETIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SETIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SETIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SETIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SETIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SETIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SETIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SET),
				new Z80CPU.FXCBOPDO(this.FXCB_SETIX),
				new Z80CPU.FXCBOPDO(this.FXCB_SET)
			};
		}

		public static string GetMnemonic(Z80CPU.MEMREADER MemReader, int Addr, bool Hex, out int MnemLength)
		{
			int num = 0;
			byte b = MemReader((ushort)(Addr + num));
			MnemLength = 1;
			string text = "*prefix*";
			int num2 = 0;
			string text2;
			if (b == 203)
			{
				num++;
				MnemLength++;
				text2 = Z80CPU.CBhZ80Code[(int)MemReader((ushort)(Addr + num))];
			}
			else if (b == 237)
			{
				num++;
				MnemLength++;
				text2 = Z80CPU.EDhZ80Code[(int)MemReader((ushort)(Addr + num))];
				if (text2.Length == 0)
				{
					text2 = "*NOP";
				}
			}
			else if (b == 221 || b == 253)
			{
				for (;;)
				{
					b = MemReader((ushort)(Addr + num));
					if (b == 221)
					{
						text = "IX";
					}
					else
					{
						text = "IY";
					}
					num++;
					num2 = 1;
					MnemLength++;
					if (MemReader((ushort)(Addr + num)) == 203)
					{
						break;
					}
					if (MemReader((ushort)(Addr + num)) == 237)
					{
						goto Block_8;
					}
					if (MemReader((ushort)(Addr + num)) != 221 && MemReader((ushort)(Addr + num)) != 253)
					{
						goto Block_12;
					}
				}
				num++;
				MnemLength++;
				num2 = 0;
				text2 = Z80CPU.DDFDCBhZ80Code[(int)MemReader((ushort)(Addr + num + 1))];
				if (text2.Length == 0)
				{
					MnemLength++;
					goto IL_19E;
				}
				goto IL_19E;
				Block_8:
				num++;
				MnemLength++;
				text2 = Z80CPU.EDhZ80Code[(int)MemReader((ushort)(Addr + num))];
				if (text2.Length == 0)
				{
					text2 = "*NOP";
				}
				if (text2[0] != '*')
				{
					text2 = "*" + text2;
					goto IL_19E;
				}
				goto IL_19E;
				Block_12:
				text2 = Z80CPU.DDFDhZ80Code[(int)MemReader((ushort)(Addr + num))];
				IL_19E:
				if (text2.Length == 0)
				{
					text2 = "*" + Z80CPU.DirectZ80Code[(int)MemReader((ushort)(Addr + num))];
				}
			}
			else
			{
				text2 = Z80CPU.DirectZ80Code[(int)MemReader((ushort)(Addr + num))];
			}
			if (text2.IndexOf("$") < 0)
			{
				return text2;
			}
			do
			{
				if (text2.IndexOf("$R") >= 0)
				{
					int num3 = text2.IndexOf("$R");
					if (text2.Length <= num3 + 1 + 1)
					{
						text2 = text2.Remove(num3, 2);
						text2 = text2.Insert(num3, text);
					}
					else if (text2[num3 + 2] == 'L' || text2[num3 + 2] == 'H')
					{
						text2 = text2.Remove(num3, 2);
						text2 = text2.Insert(num3, "" + text[1]);
					}
					else
					{
						text2 = text2.Remove(num3, 2);
						text2 = text2.Insert(num3, text);
					}
				}
				if (text2.IndexOf("$PLUS") >= 0)
				{
					sbyte b2 = (sbyte)MemReader((ushort)(Addr + (num + num2)));
					int num4 = (int)b2;
					if (b2 < 0)
					{
						num4 = -num4;
					}
					string value;
					if (b2 < 0)
					{
						if (Hex)
						{
							value = "-#" + num4.ToString("X2");
						}
						else
						{
							value = "-" + num4.ToString();
						}
					}
					else if (Hex)
					{
						value = "+#" + num4.ToString("X2");
					}
					else
					{
						value = "+" + num4.ToString();
					}
					int num3 = text2.IndexOf("$PLUS");
					text2 = text2.Remove(num3, 5);
					text2 = text2.Insert(num3, value);
					MnemLength++;
					num++;
				}
				if (text2.IndexOf("$S") >= 0)
				{
					byte b3 = MemReader((ushort)(Addr + num));
					string value = ((b3 & 56) >> 3).ToString();
					int num3 = text2.IndexOf("$S");
					text2 = text2.Remove(num3, 2);
					text2 = text2.Insert(num3, value);
				}
				if (text2.IndexOf("$W") >= 0)
				{
					ushort num5 = (ushort)((int)MemReader((ushort)(Addr + num + 1)) + 256 * (int)MemReader((ushort)(Addr + num + 2)));
					string value;
					if (Hex)
					{
						value = "#" + num5.ToString("X4");
					}
					else
					{
						value = num5.ToString();
					}
					int num3 = text2.IndexOf("$W");
					text2 = text2.Remove(num3, 2);
					text2 = text2.Insert(num3, value);
					MnemLength += 2;
				}
				if (text2.IndexOf("$N") >= 0)
				{
					byte b4 = MemReader((ushort)(Addr + num + 1));
					string value;
					if (Hex)
					{
						value = "#" + b4.ToString("X2");
					}
					else
					{
						value = b4.ToString();
					}
					int num3 = text2.IndexOf("$N");
					text2 = text2.Remove(num3, 2);
					text2 = text2.Insert(num3, value);
					MnemLength++;
				}
				if (text2.IndexOf("$T") >= 0)
				{
					byte b5 = MemReader((ushort)(Addr + num));
					int num6 = ((b5 & 56) >> 3) * 8;
					string value;
					if (Hex)
					{
						value = "#" + num6.ToString("X2");
					}
					else
					{
						value = num6.ToString();
					}
					int num3 = text2.IndexOf("$T");
					text2 = text2.Remove(num3, 2);
					text2 = text2.Insert(num3, value);
				}
				if (text2.IndexOf("$DIS") >= 0)
				{
					sbyte b6 = (sbyte)MemReader((ushort)(Addr + num + 1));
					int num7 = Addr + 2 + num + (int)b6;
					num7 = (int)((ushort)num7);
					string value;
					if (Hex)
					{
						value = "#" + num7.ToString("X4");
					}
					else
					{
						value = num7.ToString();
					}
					int num3 = text2.IndexOf("$DIS");
					text2 = text2.Remove(num3, 4);
					text2 = text2.Insert(num3, value);
					MnemLength++;
				}
			}
			while (text2.IndexOf("$") >= 0);
			return text2;
		}

		private void ALU_ADDR(byte src)
		{
			this.regs.F = this.adcf[(int)this.regs.A + (int)src * 256];
			Registers registers = this.regs;
			registers.A += src;
		}

		private void ALU_ADCR(byte src)
		{
			byte b = this.regs.F & 1;
			this.regs.F = this.adcf[(int)this.regs.A + (int)src * 256 + 65536 * (int)b];
			Registers registers = this.regs;
			registers.A += src + b;
		}

		private void ALU_SUBR(byte src)
		{
			this.regs.F = this.sbcf[(int)this.regs.A * 256 + (int)src];
			Registers registers = this.regs;
			registers.A -= src;
		}

		private void ALU_SBCR(byte src)
		{
			byte b = this.regs.F & 1;
			this.regs.F = this.sbcf[(int)this.regs.A * 256 + (int)src + 65536 * (int)b];
			Registers registers = this.regs;
			registers.A -= src + b;
		}

		private void ALU_ANDR(byte src)
		{
			Registers registers = this.regs;
			registers.A &= src;
			this.regs.F = (this.log_f[(int)this.regs.A] | 16);
		}

		private void ALU_XORR(byte src)
		{
			Registers registers = this.regs;
			registers.A ^= src;
			this.regs.F = this.log_f[(int)this.regs.A];
		}

		private void ALU_ORR(byte src)
		{
			Registers registers = this.regs;
			registers.A |= src;
			this.regs.F = this.log_f[(int)this.regs.A];
		}

		private void ALU_CPR(byte src)
		{
			this.regs.F = this.cpf[(int)this.regs.A * 256 + (int)src];
		}

		private byte ALU_INCR(byte x)
		{
			this.regs.F = (Z80CPU.incf[(int)x] | (this.regs.F & 1));
			x += 1;
			return x;
		}

		private byte ALU_DECR(byte x)
		{
			this.regs.F = (Z80CPU.decf[(int)x] | (this.regs.F & 1));
			x -= 1;
			return x;
		}

		private ushort ALU_ADDHLRR(ushort rhl, ushort rde)
		{
			this.regs.F = (byte)((int)this.regs.F & -60);
			Registers registers = this.regs;
			registers.F |= (byte)((rhl & 4095) + (rde & 4095) >> 8 & 16);
			uint num = (uint)((rhl & ushort.MaxValue) + (rde & ushort.MaxValue));
			if ((num & 65536U) != 0U)
			{
				Registers registers2 = this.regs;
				registers2.F |= 1;
			}
			Registers registers3 = this.regs;
			registers3.F |= ((byte)(num >> 8 & 255U) & 40);
			return (ushort)(num & 65535U);
		}

		private byte ALU_RLC(int x)
		{
			this.regs.F = Z80CPU.rlcf[x];
			x <<= 1;
			if ((x & 256) != 0)
			{
				x = ((x | 1) & 255);
			}
			return (byte)x;
		}

		private byte ALU_RRC(int x)
		{
			this.regs.F = Z80CPU.rrcf[x];
			if ((x & 1) != 0)
			{
				x = (x >> 1 | 128);
			}
			else
			{
				x >>= 1;
			}
			return (byte)x;
		}

		private byte ALU_RL(int x)
		{
			if ((this.regs.F & 1) != 0)
			{
				this.regs.F = Z80CPU.rl1[x];
				x <<= 1;
				x++;
			}
			else
			{
				this.regs.F = Z80CPU.rl0[x];
				x <<= 1;
			}
			return (byte)(x & 255);
		}

		private byte ALU_RR(int x)
		{
			if ((this.regs.F & 1) != 0)
			{
				this.regs.F = Z80CPU.rr1[x];
				x >>= 1;
				x += 128;
			}
			else
			{
				this.regs.F = Z80CPU.rr0[x];
				x >>= 1;
			}
			return (byte)(x & 255);
		}

		private byte ALU_SLA(int x)
		{
			this.regs.F = Z80CPU.rl0[x];
			x <<= 1;
			return (byte)x;
		}

		private byte ALU_SRA(int x)
		{
			this.regs.F = Z80CPU.sraf[x];
			x = (x >> 1) + (x & 128);
			return (byte)x;
		}

		private byte ALU_SLL(int x)
		{
			this.regs.F = Z80CPU.rl1[x];
			x <<= 1;
			x++;
			return (byte)(x & 255);
		}

		private byte ALU_SRL(int x)
		{
			this.regs.F = Z80CPU.rr0[x];
			x >>= 1;
			return (byte)x;
		}

		private void ALU_BIT(byte src, int bit)
		{
			this.regs.F = (this.log_f[(int)src & 1 << bit] | 16 | (this.regs.F & 1) | (src & 40));
		}

		private void ALU_BITMEM(byte src, int bit)
		{
			this.regs.F = (this.log_f[(int)src & 1 << bit] | 16 | (this.regs.F & 1));
			this.regs.F = ((this.regs.F & 215) | (this.regs.MH & 40));
		}

		private void ALU_INIT()
		{
			Z80CPU.alualg = new Z80CPU.ALUALGORITHM[]
			{
				new Z80CPU.ALUALGORITHM(this.ALU_ADDR),
				new Z80CPU.ALUALGORITHM(this.ALU_ADCR),
				new Z80CPU.ALUALGORITHM(this.ALU_SUBR),
				new Z80CPU.ALUALGORITHM(this.ALU_SBCR),
				new Z80CPU.ALUALGORITHM(this.ALU_ANDR),
				new Z80CPU.ALUALGORITHM(this.ALU_XORR),
				new Z80CPU.ALUALGORITHM(this.ALU_ORR),
				new Z80CPU.ALUALGORITHM(this.ALU_CPR)
			};
			this.alulogic = new Z80CPU.ALUALGORITHM[]
			{
				new Z80CPU.ALUALGORITHM(this.RLCA),
				new Z80CPU.ALUALGORITHM(this.RRCA),
				new Z80CPU.ALUALGORITHM(this.RLA),
				new Z80CPU.ALUALGORITHM(this.RRA),
				new Z80CPU.ALUALGORITHM(this.DAA),
				new Z80CPU.ALUALGORITHM(this.CPL),
				new Z80CPU.ALUALGORITHM(this.SCF),
				new Z80CPU.ALUALGORITHM(this.CCF)
			};
			this.make_adc();
			this.make_sbc();
			this.make_log();
			this.make_rot();
		}

		private void make_log()
		{
			this.log_f = new byte[256];
			for (int i = 0; i < 256; i++)
			{
				byte b = (byte)(i & 168);
				byte b2 = 4;
				for (int num = 128; num != 0; num /= 2)
				{
					if ((i & num) == num)
					{
						b2 ^= 4;
					}
				}
				this.log_f[i] = (b | b2);
			}
			byte[] array = this.log_f;
			int num2 = 0;
			array[num2] |= 64;
		}

		private void make_sbc()
		{
			this.sbcf = new byte[131072];
			this.cpf = new byte[65536];
			this.cpf8b = new byte[65536];
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 256; j++)
				{
					for (int k = 0; k < 256; k++)
					{
						int num = j - k - i;
						byte b = (byte)(num & 168);
						if ((num & 255) == 0)
						{
							b |= 64;
						}
						if ((num & 65536) != 0)
						{
							b |= 1;
						}
						int num2 = (int)((sbyte)j - (sbyte)k) - i;
						if (num2 >= 128 || num2 < -128)
						{
							b |= 4;
						}
						if (((j & 15) - (num & 15) - i & 16) != 0)
						{
							b |= 16;
						}
						b |= 2;
						this.sbcf[i * 65536 + j * 256 + k] = b;
					}
				}
			}
			for (int l = 0; l < 65536; l++)
			{
				this.cpf[l] = (byte)((int)(this.sbcf[l] & 215) | (l & 40));
				byte b2 = (byte)((l >> 8) - (l & 255) - ((this.sbcf[l] & 16) >> 4));
				this.cpf8b[l] = (byte)((int)((this.sbcf[l] & 210) + (b2 & 8)) + ((int)b2 << 4 & 32));
			}
		}

		private void make_adc()
		{
			this.adcf = new byte[131072];
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 256; j++)
				{
					for (int k = 0; k < 256; k++)
					{
						uint num = (uint)(j + k + i);
						byte b = 0;
						if ((num & 255U) == 0U)
						{
							b |= 64;
						}
						b |= (byte)(num & 168U);
						if (num >= 256U)
						{
							b |= 1;
						}
						if (((j & 15) + (k & 15) + i & 16) != 0)
						{
							b |= 16;
						}
						int num2 = (int)((sbyte)j + (sbyte)k) + i;
						if (num2 >= 128 || num2 <= -129)
						{
							b |= 4;
						}
						this.adcf[i * 65536 + j * 256 + k] = b;
					}
				}
			}
		}

		private void make_rot()
		{
			this.rlcaf = new byte[256];
			this.rrcaf = new byte[256];
			for (int i = 0; i < 256; i++)
			{
				this.rlcaf[i] = (Z80CPU.rlcf[i] & 59);
			}
			for (int j = 0; j < 256; j++)
			{
				this.rrcaf[j] = (Z80CPU.rrcf[j] & 59);
			}
		}

		public ulong Tact;

		public Registers regs = new Registers();

		public bool HALTED;

		public bool IFF1;

		public bool IFF2;

		public byte IM;

		public bool BlockINT;

		public OPFX FX;

		public OPXFX XFX;

		public bool INT;

		public bool NMI;

		public bool RST;

		public byte FreeBUS = byte.MaxValue;

		public OnRDMEM ReadMemory;

		public OnWRMEM WriteMemory;

		public OnRDPORT ReadPort;

		public OnWRPORT WritePort;

		public OnCALLBACK OnCycle;

		private Z80CPU.XFXOPDO[] opTABLE;

		private Z80CPU.XFXOPDO[] fxopTABLE;

		private Z80CPU.XFXOPDO[] edopTABLE;

		private Z80CPU.XFXOPDO[] cbopTABLE;

		private Z80CPU.FXCBOPDO[] fxcbopTABLE;

		private static byte[] conds = new byte[]
		{
			64,
			1,
			4,
			128
		};

		private static Z80CPU.ALUALGORITHM[] alualg;

		private Z80CPU.ALUALGORITHM[] alulogic;

		private static readonly string[] DirectZ80Code = new string[]
		{
			"NOP",
			"LD     BC,$W",
			"LD     (BC),A",
			"INC    BC",
			"INC    B",
			"DEC    B",
			"LD     B,$N",
			"RLCA",
			"EX     AF,AF'",
			"ADD    HL,BC",
			"LD     A,(BC)",
			"DEC    BC",
			"INC    C",
			"DEC    C",
			"LD     C,$N",
			"RRCA",
			"DJNZ   $DIS",
			"LD     DE,$W",
			"LD     (DE),A",
			"INC    DE",
			"INC    D",
			"DEC    D",
			"LD     D,$N",
			"RLA",
			"JR     $DIS",
			"ADD    HL,DE",
			"LD     A,(DE)",
			"DEC    DE",
			"INC    E",
			"DEC    E",
			"LD     E,$N",
			"RRA",
			"JR     NZ,$DIS",
			"LD     HL,$W",
			"LD     ($W),HL",
			"INC    HL",
			"INC    H",
			"DEC    H",
			"LD     H,$N",
			"DAA",
			"JR     Z,$DIS",
			"ADD    HL,HL",
			"LD     HL,($W)",
			"DEC    HL",
			"INC    L",
			"DEC    L",
			"LD     L,$N",
			"CPL",
			"JR     NC,$DIS",
			"LD     SP,$W",
			"LD     ($W),A",
			"INC    SP",
			"INC    (HL)",
			"DEC    (HL)",
			"LD     (HL),$N",
			"SCF",
			"JR     C,$DIS",
			"ADD    HL,SP",
			"LD     A,($W)",
			"DEC    SP",
			"INC    A",
			"DEC    A",
			"LD     A,$N",
			"CCF",
			"LD     B,B",
			"LD     B,C",
			"LD     B,D",
			"LD     B,E",
			"LD     B,H",
			"LD     B,L",
			"LD     B,(HL)",
			"LD     B,A",
			"LD     C,B",
			"LD     C,C",
			"LD     C,D",
			"LD     C,E",
			"LD     C,H",
			"LD     C,L",
			"LD     C,(HL)",
			"LD     C,A",
			"LD     D,B",
			"LD     D,C",
			"LD     D,D",
			"LD     D,E",
			"LD     D,H",
			"LD     D,L",
			"LD     D,(HL)",
			"LD     D,A",
			"LD     E,B",
			"LD     E,C",
			"LD     E,D",
			"LD     E,E",
			"LD     E,H",
			"LD     E,L",
			"LD     E,(HL)",
			"LD     E,A",
			"LD     H,B",
			"LD     H,C",
			"LD     H,D",
			"LD     H,E",
			"LD     H,H",
			"LD     H,L",
			"LD     H,(HL)",
			"LD     H,A",
			"LD     L,B",
			"LD     L,C",
			"LD     L,D",
			"LD     L,E",
			"LD     L,H",
			"LD     L,L",
			"LD     L,(HL)",
			"LD     L,A",
			"LD     (HL),B",
			"LD     (HL),C",
			"LD     (HL),D",
			"LD     (HL),E",
			"LD     (HL),H",
			"LD     (HL),L",
			"HALT",
			"LD     (HL),A",
			"LD     A,B",
			"LD     A,C",
			"LD     A,D",
			"LD     A,E",
			"LD     A,H",
			"LD     A,L",
			"LD     A,(HL)",
			"LD     A,A",
			"ADD    A,B",
			"ADD    A,C",
			"ADD    A,D",
			"ADD    A,E",
			"ADD    A,H",
			"ADD    A,L",
			"ADD    A,(HL)",
			"ADD    A,A",
			"ADC    A,B",
			"ADC    A,C",
			"ADC    A,D",
			"ADC    A,E",
			"ADC    A,H",
			"ADC    A,L",
			"ADC    A,(HL)",
			"ADC    A,A",
			"SUB    B",
			"SUB    C",
			"SUB    D",
			"SUB    E",
			"SUB    H",
			"SUB    L",
			"SUB    (HL)",
			"SUB    A",
			"SBC    A,B",
			"SBC    A,C",
			"SBC    A,D",
			"SBC    A,E",
			"SBC    A,H",
			"SBC    A,L",
			"SBC    A,(HL)",
			"SBC    A,A",
			"AND    B",
			"AND    C",
			"AND    D",
			"AND    E",
			"AND    H",
			"AND    L",
			"AND    (HL)",
			"AND    A",
			"XOR    B",
			"XOR    C",
			"XOR    D",
			"XOR    E",
			"XOR    H",
			"XOR    L",
			"XOR    (HL)",
			"XOR    A",
			"OR     B",
			"OR     C",
			"OR     D",
			"OR     E",
			"OR     H",
			"OR     L",
			"OR     (HL)",
			"OR     A",
			"CP     B",
			"CP     C",
			"CP     D",
			"CP     E",
			"CP     H",
			"CP     L",
			"CP     (HL)",
			"CP     A",
			"RET    NZ",
			"POP    BC",
			"JP     NZ,$W",
			"JP     $W",
			"CALL   NZ,$W",
			"PUSH   BC",
			"ADD    A,$N",
			"RST    $T",
			"RET    Z",
			"RET",
			"JP     Z,$W",
			"*CB",
			"CALL   Z,$W",
			"CALL   $W",
			"ADC    A,$N",
			"RST    $T",
			"RET    NC",
			"POP    DE",
			"JP     NC,$W",
			"OUT    ($N),A",
			"CALL   NC,$W",
			"PUSH   DE",
			"SUB    $N",
			"RST    $T",
			"RET    C",
			"EXX",
			"JP     C,$W",
			"IN     A,($N)",
			"CALL   C,$W",
			"*IX",
			"SBC    A,$N",
			"RST    $T",
			"RET    PO",
			"POP    HL",
			"JP     PO,$W",
			"EX     (SP),HL",
			"CALL   PO,$W",
			"PUSH   HL",
			"AND    $N",
			"RST    $T",
			"RET    PE",
			"JP     (HL)",
			"JP     PE,$W",
			"EX     DE,HL",
			"CALL   PE,$W",
			"*ED",
			"XOR    $N",
			"RST    $T",
			"RET    P",
			"POP    AF",
			"JP     P,$W",
			"DI",
			"CALL   P,$W",
			"PUSH   AF",
			"OR     $N",
			"RST    $T",
			"RET    M",
			"LD     SP,HL",
			"JP     M,$W",
			"EI",
			"CALL   M,$W",
			"*IY",
			"CP     $N",
			"RST    $T"
		};

		private static readonly string[] CBhZ80Code = new string[]
		{
			"RLC    B",
			"RLC    C",
			"RLC    D",
			"RLC    E",
			"RLC    H",
			"RLC    L",
			"RLC    (HL)",
			"RLC    A",
			"RRC    B",
			"RRC    C",
			"RRC    D",
			"RRC    E",
			"RRC    H",
			"RRC    L",
			"RRC    (HL)",
			"RRC    A",
			"RL     B",
			"RL     C",
			"RL     D",
			"RL     E",
			"RL     H",
			"RL     L",
			"RL     (HL)",
			"RL     A",
			"RR     B",
			"RR     C",
			"RR     D",
			"RR     E",
			"RR     H",
			"RR     L",
			"RR     (HL)",
			"RR     A",
			"SLA    B",
			"SLA    C",
			"SLA    D",
			"SLA    E",
			"SLA    H",
			"SLA    L",
			"SLA    (HL)",
			"SLA    A",
			"SRA    B",
			"SRA    C",
			"SRA    D",
			"SRA    E",
			"SRA    H",
			"SRA    L",
			"SRA    (HL)",
			"SRA    A",
			"*SLL   B",
			"*SLL   C",
			"*SLL   D",
			"*SLL   E",
			"*SLL   H",
			"*SLL   L",
			"*SLL   (HL)",
			"*SLL   A",
			"SRL    B",
			"SRL    C",
			"SRL    D",
			"SRL    E",
			"SRL    H",
			"SRL    L",
			"SRL    (HL)",
			"SRL    A",
			"BIT    $S,B",
			"BIT    $S,C",
			"BIT    $S,D",
			"BIT    $S,E",
			"BIT    $S,H",
			"BIT    $S,L",
			"BIT    $S,(HL)",
			"BIT    $S,A",
			"BIT    $S,B",
			"BIT    $S,C",
			"BIT    $S,D",
			"BIT    $S,E",
			"BIT    $S,H",
			"BIT    $S,L",
			"BIT    $S,(HL)",
			"BIT    $S,A",
			"BIT    $S,B",
			"BIT    $S,C",
			"BIT    $S,D",
			"BIT    $S,E",
			"BIT    $S,H",
			"BIT    $S,L",
			"BIT    $S,(HL)",
			"BIT    $S,A",
			"BIT    $S,B",
			"BIT    $S,C",
			"BIT    $S,D",
			"BIT    $S,E",
			"BIT    $S,H",
			"BIT    $S,L",
			"BIT    $S,(HL)",
			"BIT    $S,A",
			"BIT    $S,B",
			"BIT    $S,C",
			"BIT    $S,D",
			"BIT    $S,E",
			"BIT    $S,H",
			"BIT    $S,L",
			"BIT    $S,(HL)",
			"BIT    $S,A",
			"BIT    $S,B",
			"BIT    $S,C",
			"BIT    $S,D",
			"BIT    $S,E",
			"BIT    $S,H",
			"BIT    $S,L",
			"BIT    $S,(HL)",
			"BIT    $S,A",
			"BIT    $S,B",
			"BIT    $S,C",
			"BIT    $S,D",
			"BIT    $S,E",
			"BIT    $S,H",
			"BIT    $S,L",
			"BIT    $S,(HL)",
			"BIT    $S,A",
			"BIT    $S,B",
			"BIT    $S,C",
			"BIT    $S,D",
			"BIT    $S,E",
			"BIT    $S,H",
			"BIT    $S,L",
			"BIT    $S,(HL)",
			"BIT    $S,A",
			"RES    $S,B",
			"RES    $S,C",
			"RES    $S,D",
			"RES    $S,E",
			"RES    $S,H",
			"RES    $S,L",
			"RES    $S,(HL)",
			"RES    $S,A",
			"RES    $S,B",
			"RES    $S,C",
			"RES    $S,D",
			"RES    $S,E",
			"RES    $S,H",
			"RES    $S,L",
			"RES    $S,(HL)",
			"RES    $S,A",
			"RES    $S,B",
			"RES    $S,C",
			"RES    $S,D",
			"RES    $S,E",
			"RES    $S,H",
			"RES    $S,L",
			"RES    $S,(HL)",
			"RES    $S,A",
			"RES    $S,B",
			"RES    $S,C",
			"RES    $S,D",
			"RES    $S,E",
			"RES    $S,H",
			"RES    $S,L",
			"RES    $S,(HL)",
			"RES    $S,A",
			"RES    $S,B",
			"RES    $S,C",
			"RES    $S,D",
			"RES    $S,E",
			"RES    $S,H",
			"RES    $S,L",
			"RES    $S,(HL)",
			"RES    $S,A",
			"RES    $S,B",
			"RES    $S,C",
			"RES    $S,D",
			"RES    $S,E",
			"RES    $S,H",
			"RES    $S,L",
			"RES    $S,(HL)",
			"RES    $S,A",
			"RES    $S,B",
			"RES    $S,C",
			"RES    $S,D",
			"RES    $S,E",
			"RES    $S,H",
			"RES    $S,L",
			"RES    $S,(HL)",
			"RES    $S,A",
			"RES    $S,B",
			"RES    $S,C",
			"RES    $S,D",
			"RES    $S,E",
			"RES    $S,H",
			"RES    $S,L",
			"RES    $S,(HL)",
			"RES    $S,A",
			"SET    $S,B",
			"SET    $S,C",
			"SET    $S,D",
			"SET    $S,E",
			"SET    $S,H",
			"SET    $S,L",
			"SET    $S,(HL)",
			"SET    $S,A",
			"SET    $S,B",
			"SET    $S,C",
			"SET    $S,D",
			"SET    $S,E",
			"SET    $S,H",
			"SET    $S,L",
			"SET    $S,(HL)",
			"SET    $S,A",
			"SET    $S,B",
			"SET    $S,C",
			"SET    $S,D",
			"SET    $S,E",
			"SET    $S,H",
			"SET    $S,L",
			"SET    $S,(HL)",
			"SET    $S,A",
			"SET    $S,B",
			"SET    $S,C",
			"SET    $S,D",
			"SET    $S,E",
			"SET    $S,H",
			"SET    $S,L",
			"SET    $S,(HL)",
			"SET    $S,A",
			"SET    $S,B",
			"SET    $S,C",
			"SET    $S,D",
			"SET    $S,E",
			"SET    $S,H",
			"SET    $S,L",
			"SET    $S,(HL)",
			"SET    $S,A",
			"SET    $S,B",
			"SET    $S,C",
			"SET    $S,D",
			"SET    $S,E",
			"SET    $S,H",
			"SET    $S,L",
			"SET    $S,(HL)",
			"SET    $S,A",
			"SET    $S,B",
			"SET    $S,C",
			"SET    $S,D",
			"SET    $S,E",
			"SET    $S,H",
			"SET    $S,L",
			"SET    $S,(HL)",
			"SET    $S,A",
			"SET    $S,B",
			"SET    $S,C",
			"SET    $S,D",
			"SET    $S,E",
			"SET    $S,H",
			"SET    $S,L",
			"SET    $S,(HL)",
			"SET    $S,A"
		};

		private static readonly string[] EDhZ80Code = new string[]
		{
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"IN     B,(C)",
			"OUT    (C),B",
			"SBC    HL,BC",
			"LD     ($W),BC",
			"NEG",
			"RETN",
			"IM     0",
			"LD     I,A",
			"IN     C,(C)",
			"OUT    (C),C",
			"ADC    HL,BC",
			"LD     BC,($W)",
			"*NEG",
			"RETI",
			"*IM    0",
			"LD     R,A",
			"IN     D,(C)",
			"OUT    (C),D",
			"SBC    HL,DE",
			"LD     ($W),DE",
			"*NEG",
			"*RETN",
			"IM     1",
			"LD     A,I",
			"IN     E,(C)",
			"OUT    (C),E",
			"ADC    HL,DE",
			"LD     DE,($W)",
			"*NEG",
			"*RETN",
			"IM     2",
			"LD     A,R",
			"IN     H,(C)",
			"OUT    (C),H",
			"SBC    HL,HL",
			"LD     ($W),HL",
			"*NEG",
			"*RETN",
			"*IM    0",
			"RRD",
			"IN     L,(C)",
			"OUT    (C),L",
			"ADC    HL,HL",
			"LD     HL,($W)",
			"*NEG",
			"*RETN",
			"*IM    0",
			"RLD",
			"*IN    (C)",
			"*OUT   (C),0",
			"SBC    HL,SP",
			"LD     ($W),SP",
			"*NEG",
			"*RETN",
			"*IM    1",
			"*NOP",
			"IN     A,(C)",
			"OUT    (C),A",
			"ADC    HL,SP",
			"LD     SP,($W)",
			"*NEG",
			"*RETN",
			"*IM    2",
			"*NOP",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"LDI",
			"CPI",
			"INI",
			"OUTI",
			"",
			"",
			"",
			"",
			"LDD",
			"CPD",
			"IND",
			"OUTD",
			"",
			"",
			"",
			"",
			"LDIR",
			"CPIR",
			"INIR",
			"OTIR",
			"",
			"",
			"",
			"",
			"LDDR",
			"CPDR",
			"INDR",
			"OTDR",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			""
		};

		private static readonly string[] DDFDhZ80Code = new string[]
		{
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"ADD    $R,BC",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"ADD    $R,DE",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"LD     $R,$W",
			"LD     ($W),$R",
			"INC    $R",
			"*INC   $RH",
			"*DEC   $RH",
			"*LD    $RH,$N",
			"",
			"",
			"ADD    $R,$R",
			"LD     $R,($W)",
			"DEC    $R",
			"*INC   $RL",
			"*DEC   $RL",
			"*LD    $RL,$N",
			"",
			"",
			"",
			"",
			"",
			"INC    ($R$PLUS)",
			"DEC    ($R$PLUS)",
			"LD     ($R$PLUS),$N",
			"",
			"",
			"ADD    $R,SP",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"*LD    B,$RH",
			"*LD    B,$RL",
			"LD     B,($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*LD    C,$RH",
			"*LD    C,$RL",
			"LD     C,($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*LD    D,$RH",
			"*LD    D,$RL",
			"LD     D,($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*LD    E,$RH",
			"*LD    E,$RL",
			"LD     E,($R$PLUS)",
			"",
			"*LD    $RH,B",
			"*LD    $RH,C",
			"*LD    $RH,D",
			"*LD    $RH,E",
			"*LD    $RH,$RH",
			"*LD    $RH,$RL",
			"LD     H,($R$PLUS)",
			"*LD    $RH,A",
			"*LD    $RL,B",
			"*LD    $RL,C",
			"*LD    $RL,D",
			"*LD    $RL,E",
			"*LD    $RL,$RH",
			"*LD    $RL,$RL",
			"LD     L,($R$PLUS)",
			"*LD    $RL,A",
			"LD     ($R$PLUS),B",
			"LD     ($R$PLUS),C",
			"LD     ($R$PLUS),D",
			"LD     ($R$PLUS),E",
			"LD     ($R$PLUS),H",
			"LD     ($R$PLUS),L",
			"",
			"LD     ($R$PLUS),A",
			"",
			"",
			"",
			"",
			"*LD    A,$RH",
			"*LD    A,$RL",
			"LD     A,($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*ADD   A,$RH",
			"*ADD   A,$RL",
			"ADD    A,($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*ADC   A,$RH",
			"*ADC   A,$RL",
			"ADC    A,($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*SUB   $RH",
			"*SUB   $RL",
			"SUB    ($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*SBC   A,$RH",
			"*SBC   A,$RL",
			"SBC    A,($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*AND   $RH",
			"*AND   $RL",
			"AND    ($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*XOR   $RH",
			"*XOR   $RL",
			"XOR    ($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*OR    $RH",
			"*OR    $RL",
			"OR     ($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"*CP    $RH",
			"*CP    $RL",
			"CP     ($R$PLUS)",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"*DD/FD,CB",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"*DD/FD,DD",
			"",
			"",
			"",
			"POP    $R",
			"",
			"EX     (SP),$R",
			"",
			"PUSH   $R",
			"",
			"",
			"",
			"JP     ($R)",
			"",
			"",
			"",
			"*DD/FD,ED",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"",
			"LD     SP,$R",
			"",
			"",
			"",
			"*DD/FD,FD",
			"",
			""
		};

		private static readonly string[] DDFDCBhZ80Code = new string[]
		{
			"*RLC   B,($R$PLUS)",
			"*RLC   C,($R$PLUS)",
			"*RLC   D,($R$PLUS)",
			"*RLC   E,($R$PLUS)",
			"*RLC   H,($R$PLUS)",
			"*RLC   L,($R$PLUS)",
			"RLC    ($R$PLUS)",
			"*RLC   A,($R$PLUS)",
			"*RRC   B,($R$PLUS)",
			"*RRC   C,($R$PLUS)",
			"*RRC   D,($R$PLUS)",
			"*RRC   E,($R$PLUS)",
			"*RRC   H,($R$PLUS)",
			"*RRC   L,($R$PLUS)",
			"RRC    ($R$PLUS)",
			"*RRC   A,($R$PLUS)",
			"*RL    B,($R$PLUS)",
			"*RL    C,($R$PLUS)",
			"*RL    D,($R$PLUS)",
			"*RL    E,($R$PLUS)",
			"*RL    H,($R$PLUS)",
			"*RL    L,($R$PLUS)",
			"RL     ($R$PLUS)",
			"*RL    A,($R$PLUS)",
			"*RR    B,($R$PLUS)",
			"*RR    C,($R$PLUS)",
			"*RR    D,($R$PLUS)",
			"*RR    E,($R$PLUS)",
			"*RR    H,($R$PLUS)",
			"*RR    L,($R$PLUS)",
			"RR     ($R$PLUS)",
			"*RR    A,($R$PLUS)",
			"*SLA   B,($R$PLUS)",
			"*SLA   C,($R$PLUS)",
			"*SLA   D,($R$PLUS)",
			"*SLA   E,($R$PLUS)",
			"*SLA   H,($R$PLUS)",
			"*SLA   L,($R$PLUS)",
			"SLA    ($R$PLUS)",
			"*SLA   A,($R$PLUS)",
			"*SRA   B,($R$PLUS)",
			"*SRA   C,($R$PLUS)",
			"*SRA   D,($R$PLUS)",
			"*SRA   E,($R$PLUS)",
			"*SRA   H,($R$PLUS)",
			"*SRA   L,($R$PLUS)",
			"SRA    ($R$PLUS)",
			"*SRA   A,($R$PLUS)",
			"*SLL   B,($R$PLUS)",
			"*SLL   C,($R$PLUS)",
			"*SLL   D,($R$PLUS)",
			"*SLL   E,($R$PLUS)",
			"*SLL   H,($R$PLUS)",
			"*SLL   L,($R$PLUS)",
			"*SLL   ($R$PLUS)",
			"*SLL   A,($R$PLUS)",
			"*SRL   B,($R$PLUS)",
			"*SRL   C,($R$PLUS)",
			"*SRL   D,($R$PLUS)",
			"*SRL   E,($R$PLUS)",
			"*SRL   H,($R$PLUS)",
			"*SRL   L,($R$PLUS)",
			"SRL    ($R$PLUS)",
			"*SRL   A,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"BIT    $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"BIT    $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"BIT    $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"BIT    $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"BIT    $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"BIT    $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"BIT    $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"BIT    $S,($R$PLUS)",
			"*BIT   $S,($R$PLUS)",
			"*RES   $S,B,($R$PLUS)",
			"*RES   $S,C,($R$PLUS)",
			"*RES   $S,D,($R$PLUS)",
			"*RES   $S,E,($R$PLUS)",
			"*RES   $S,H,($R$PLUS)",
			"*RES   $S,L,($R$PLUS)",
			"RES    $S,($R$PLUS)",
			"*RES   $S,A,($R$PLUS)",
			"*RES   $S,B,($R$PLUS)",
			"*RES   $S,C,($R$PLUS)",
			"*RES   $S,D,($R$PLUS)",
			"*RES   $S,E,($R$PLUS)",
			"*RES   $S,H,($R$PLUS)",
			"*RES   $S,L,($R$PLUS)",
			"RES    $S,($R$PLUS)",
			"*RES   $S,A,($R$PLUS)",
			"*RES   $S,B,($R$PLUS)",
			"*RES   $S,C,($R$PLUS)",
			"*RES   $S,D,($R$PLUS)",
			"*RES   $S,E,($R$PLUS)",
			"*RES   $S,H,($R$PLUS)",
			"*RES   $S,L,($R$PLUS)",
			"RES    $S,($R$PLUS)",
			"*RES   $S,A,($R$PLUS)",
			"*RES   $S,B,($R$PLUS)",
			"*RES   $S,C,($R$PLUS)",
			"*RES   $S,D,($R$PLUS)",
			"*RES   $S,E,($R$PLUS)",
			"*RES   $S,H,($R$PLUS)",
			"*RES   $S,L,($R$PLUS)",
			"RES    $S,($R$PLUS)",
			"*RES   $S,A,($R$PLUS)",
			"*RES   $S,B,($R$PLUS)",
			"*RES   $S,C,($R$PLUS)",
			"*RES   $S,D,($R$PLUS)",
			"*RES   $S,E,($R$PLUS)",
			"*RES   $S,H,($R$PLUS)",
			"*RES   $S,L,($R$PLUS)",
			"RES    $S,($R$PLUS)",
			"*RES   $S,A,($R$PLUS)",
			"*RES   $S,B,($R$PLUS)",
			"*RES   $S,C,($R$PLUS)",
			"*RES   $S,D,($R$PLUS)",
			"*RES   $S,E,($R$PLUS)",
			"*RES   $S,H,($R$PLUS)",
			"*RES   $S,L,($R$PLUS)",
			"RES    $S,($R$PLUS)",
			"*RES   $S,A,($R$PLUS)",
			"*RES   $S,B,($R$PLUS)",
			"*RES   $S,C,($R$PLUS)",
			"*RES   $S,D,($R$PLUS)",
			"*RES   $S,E,($R$PLUS)",
			"*RES   $S,H,($R$PLUS)",
			"*RES   $S,L,($R$PLUS)",
			"RES    $S,($R$PLUS)",
			"*RES   $S,A,($R$PLUS)",
			"*RES   $S,B,($R$PLUS)",
			"*RES   $S,C,($R$PLUS)",
			"*RES   $S,D,($R$PLUS)",
			"*RES   $S,E,($R$PLUS)",
			"*RES   $S,H,($R$PLUS)",
			"*RES   $S,L,($R$PLUS)",
			"RES    $S,($R$PLUS)",
			"*RES   $S,A,($R$PLUS)",
			"*SET   $S,B,($R$PLUS)",
			"*SET   $S,C,($R$PLUS)",
			"*SET   $S,D,($R$PLUS)",
			"*SET   $S,E,($R$PLUS)",
			"*SET   $S,H,($R$PLUS)",
			"*SET   $S,L,($R$PLUS)",
			"SET    $S,($R$PLUS)",
			"*SET   $S,A,($R$PLUS)",
			"*SET   $S,B,($R$PLUS)",
			"*SET   $S,C,($R$PLUS)",
			"*SET   $S,D,($R$PLUS)",
			"*SET   $S,E,($R$PLUS)",
			"*SET   $S,H,($R$PLUS)",
			"*SET   $S,L,($R$PLUS)",
			"SET    $S,($R$PLUS)",
			"*SET   $S,A,($R$PLUS)",
			"*SET   $S,B,($R$PLUS)",
			"*SET   $S,C,($R$PLUS)",
			"*SET   $S,D,($R$PLUS)",
			"*SET   $S,E,($R$PLUS)",
			"*SET   $S,H,($R$PLUS)",
			"*SET   $S,L,($R$PLUS)",
			"SET    $S,($R$PLUS)",
			"*SET   $S,A,($R$PLUS)",
			"*SET   $S,B,($R$PLUS)",
			"*SET   $S,C,($R$PLUS)",
			"*SET   $S,D,($R$PLUS)",
			"*SET   $S,E,($R$PLUS)",
			"*SET   $S,H,($R$PLUS)",
			"*SET   $S,L,($R$PLUS)",
			"SET    $S,($R$PLUS)",
			"*SET   $S,A,($R$PLUS)",
			"*SET   $S,B,($R$PLUS)",
			"*SET   $S,C,($R$PLUS)",
			"*SET   $S,D,($R$PLUS)",
			"*SET   $S,E,($R$PLUS)",
			"*SET   $S,H,($R$PLUS)",
			"*SET   $S,L,($R$PLUS)",
			"SET    $S,($R$PLUS)",
			"*SET   $S,A,($R$PLUS)",
			"*SET   $S,B,($R$PLUS)",
			"*SET   $S,C,($R$PLUS)",
			"*SET   $S,D,($R$PLUS)",
			"*SET   $S,E,($R$PLUS)",
			"*SET   $S,H,($R$PLUS)",
			"*SET   $S,L,($R$PLUS)",
			"SET    $S,($R$PLUS)",
			"*SET   $S,A,($R$PLUS)",
			"*SET   $S,B,($R$PLUS)",
			"*SET   $S,C,($R$PLUS)",
			"*SET   $S,D,($R$PLUS)",
			"*SET   $S,E,($R$PLUS)",
			"*SET   $S,H,($R$PLUS)",
			"*SET   $S,L,($R$PLUS)",
			"SET    $S,($R$PLUS)",
			"*SET   $S,A,($R$PLUS)",
			"*SET   $S,B,($R$PLUS)",
			"*SET   $S,C,($R$PLUS)",
			"*SET   $S,D,($R$PLUS)",
			"*SET   $S,E,($R$PLUS)",
			"*SET   $S,H,($R$PLUS)",
			"*SET   $S,L,($R$PLUS)",
			"SET    $S,($R$PLUS)",
			"*SET   $S,A,($R$PLUS)"
		};

		private byte[] log_f;

		private byte[] sbcf;

		private byte[] cpf;

		private byte[] cpf8b;

		private byte[] adcf;

		private byte[] rlcaf;

		private byte[] rrcaf;

		private static byte[] rlcf = new byte[]
		{
			68,
			0,
			0,
			4,
			8,
			12,
			12,
			8,
			0,
			4,
			4,
			0,
			12,
			8,
			8,
			12,
			32,
			36,
			36,
			32,
			44,
			40,
			40,
			44,
			36,
			32,
			32,
			36,
			40,
			44,
			44,
			40,
			0,
			4,
			4,
			0,
			12,
			8,
			8,
			12,
			4,
			0,
			0,
			4,
			8,
			12,
			12,
			8,
			36,
			32,
			32,
			36,
			40,
			44,
			44,
			40,
			32,
			36,
			36,
			32,
			44,
			40,
			40,
			44,
			128,
			132,
			132,
			128,
			140,
			136,
			136,
			140,
			132,
			128,
			128,
			132,
			136,
			140,
			140,
			136,
			164,
			160,
			160,
			164,
			168,
			172,
			172,
			168,
			160,
			164,
			164,
			160,
			172,
			168,
			168,
			172,
			132,
			128,
			128,
			132,
			136,
			140,
			140,
			136,
			128,
			132,
			132,
			128,
			140,
			136,
			136,
			140,
			160,
			164,
			164,
			160,
			172,
			168,
			168,
			172,
			164,
			160,
			160,
			164,
			168,
			172,
			172,
			168,
			1,
			5,
			5,
			1,
			13,
			9,
			9,
			13,
			5,
			1,
			1,
			5,
			9,
			13,
			13,
			9,
			37,
			33,
			33,
			37,
			41,
			45,
			45,
			41,
			33,
			37,
			37,
			33,
			45,
			41,
			41,
			45,
			5,
			1,
			1,
			5,
			9,
			13,
			13,
			9,
			1,
			5,
			5,
			1,
			13,
			9,
			9,
			13,
			33,
			37,
			37,
			33,
			45,
			41,
			41,
			45,
			37,
			33,
			33,
			37,
			41,
			45,
			45,
			41,
			133,
			129,
			129,
			133,
			137,
			141,
			141,
			137,
			129,
			133,
			133,
			129,
			141,
			137,
			137,
			141,
			161,
			165,
			165,
			161,
			173,
			169,
			169,
			173,
			165,
			161,
			161,
			165,
			169,
			173,
			173,
			169,
			129,
			133,
			133,
			129,
			141,
			137,
			137,
			141,
			133,
			129,
			129,
			133,
			137,
			141,
			141,
			137,
			165,
			161,
			161,
			165,
			169,
			173,
			173,
			169,
			161,
			165,
			165,
			161,
			173,
			169,
			169,
			173
		};

		private static byte[] rrcf = new byte[]
		{
			68,
			129,
			0,
			133,
			0,
			133,
			4,
			129,
			0,
			133,
			4,
			129,
			4,
			129,
			0,
			133,
			8,
			141,
			12,
			137,
			12,
			137,
			8,
			141,
			12,
			137,
			8,
			141,
			8,
			141,
			12,
			137,
			0,
			133,
			4,
			129,
			4,
			129,
			0,
			133,
			4,
			129,
			0,
			133,
			0,
			133,
			4,
			129,
			12,
			137,
			8,
			141,
			8,
			141,
			12,
			137,
			8,
			141,
			12,
			137,
			12,
			137,
			8,
			141,
			32,
			165,
			36,
			161,
			36,
			161,
			32,
			165,
			36,
			161,
			32,
			165,
			32,
			165,
			36,
			161,
			44,
			169,
			40,
			173,
			40,
			173,
			44,
			169,
			40,
			173,
			44,
			169,
			44,
			169,
			40,
			173,
			36,
			161,
			32,
			165,
			32,
			165,
			36,
			161,
			32,
			165,
			36,
			161,
			36,
			161,
			32,
			165,
			40,
			173,
			44,
			169,
			44,
			169,
			40,
			173,
			44,
			169,
			40,
			173,
			40,
			173,
			44,
			169,
			0,
			133,
			4,
			129,
			4,
			129,
			0,
			133,
			4,
			129,
			0,
			133,
			0,
			133,
			4,
			129,
			12,
			137,
			8,
			141,
			8,
			141,
			12,
			137,
			8,
			141,
			12,
			137,
			12,
			137,
			8,
			141,
			4,
			129,
			0,
			133,
			0,
			133,
			4,
			129,
			0,
			133,
			4,
			129,
			4,
			129,
			0,
			133,
			8,
			141,
			12,
			137,
			12,
			137,
			8,
			141,
			12,
			137,
			8,
			141,
			8,
			141,
			12,
			137,
			36,
			161,
			32,
			165,
			32,
			165,
			36,
			161,
			32,
			165,
			36,
			161,
			36,
			161,
			32,
			165,
			40,
			173,
			44,
			169,
			44,
			169,
			40,
			173,
			44,
			169,
			40,
			173,
			40,
			173,
			44,
			169,
			32,
			165,
			36,
			161,
			36,
			161,
			32,
			165,
			36,
			161,
			32,
			165,
			32,
			165,
			36,
			161,
			44,
			169,
			40,
			173,
			40,
			173,
			44,
			169,
			40,
			173,
			44,
			169,
			44,
			169,
			40,
			173
		};

		private static byte[] incf = new byte[]
		{
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			16,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			48,
			32,
			32,
			32,
			32,
			32,
			32,
			32,
			40,
			40,
			40,
			40,
			40,
			40,
			40,
			40,
			48,
			32,
			32,
			32,
			32,
			32,
			32,
			32,
			40,
			40,
			40,
			40,
			40,
			40,
			40,
			40,
			16,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			16,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			8,
			48,
			32,
			32,
			32,
			32,
			32,
			32,
			32,
			40,
			40,
			40,
			40,
			40,
			40,
			40,
			40,
			48,
			32,
			32,
			32,
			32,
			32,
			32,
			32,
			40,
			40,
			40,
			40,
			40,
			40,
			40,
			40,
			148,
			128,
			128,
			128,
			128,
			128,
			128,
			128,
			136,
			136,
			136,
			136,
			136,
			136,
			136,
			136,
			144,
			128,
			128,
			128,
			128,
			128,
			128,
			128,
			136,
			136,
			136,
			136,
			136,
			136,
			136,
			136,
			176,
			160,
			160,
			160,
			160,
			160,
			160,
			160,
			168,
			168,
			168,
			168,
			168,
			168,
			168,
			168,
			176,
			160,
			160,
			160,
			160,
			160,
			160,
			160,
			168,
			168,
			168,
			168,
			168,
			168,
			168,
			168,
			144,
			128,
			128,
			128,
			128,
			128,
			128,
			128,
			136,
			136,
			136,
			136,
			136,
			136,
			136,
			136,
			144,
			128,
			128,
			128,
			128,
			128,
			128,
			128,
			136,
			136,
			136,
			136,
			136,
			136,
			136,
			136,
			176,
			160,
			160,
			160,
			160,
			160,
			160,
			160,
			168,
			168,
			168,
			168,
			168,
			168,
			168,
			168,
			176,
			160,
			160,
			160,
			160,
			160,
			160,
			160,
			168,
			168,
			168,
			168,
			168,
			168,
			168,
			168,
			80
		};

		private static byte[] decf = new byte[]
		{
			186,
			66,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			26,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			26,
			34,
			34,
			34,
			34,
			34,
			34,
			34,
			34,
			42,
			42,
			42,
			42,
			42,
			42,
			42,
			58,
			34,
			34,
			34,
			34,
			34,
			34,
			34,
			34,
			42,
			42,
			42,
			42,
			42,
			42,
			42,
			58,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			26,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			2,
			10,
			10,
			10,
			10,
			10,
			10,
			10,
			26,
			34,
			34,
			34,
			34,
			34,
			34,
			34,
			34,
			42,
			42,
			42,
			42,
			42,
			42,
			42,
			58,
			34,
			34,
			34,
			34,
			34,
			34,
			34,
			34,
			42,
			42,
			42,
			42,
			42,
			42,
			42,
			62,
			130,
			130,
			130,
			130,
			130,
			130,
			130,
			130,
			138,
			138,
			138,
			138,
			138,
			138,
			138,
			154,
			130,
			130,
			130,
			130,
			130,
			130,
			130,
			130,
			138,
			138,
			138,
			138,
			138,
			138,
			138,
			154,
			162,
			162,
			162,
			162,
			162,
			162,
			162,
			162,
			170,
			170,
			170,
			170,
			170,
			170,
			170,
			186,
			162,
			162,
			162,
			162,
			162,
			162,
			162,
			162,
			170,
			170,
			170,
			170,
			170,
			170,
			170,
			186,
			130,
			130,
			130,
			130,
			130,
			130,
			130,
			130,
			138,
			138,
			138,
			138,
			138,
			138,
			138,
			154,
			130,
			130,
			130,
			130,
			130,
			130,
			130,
			130,
			138,
			138,
			138,
			138,
			138,
			138,
			138,
			154,
			162,
			162,
			162,
			162,
			162,
			162,
			162,
			162,
			170,
			170,
			170,
			170,
			170,
			170,
			170,
			186,
			162,
			162,
			162,
			162,
			162,
			162,
			162,
			162,
			170,
			170,
			170,
			170,
			170,
			170,
			170
		};

		private static ushort[] daatab = new ushort[]
		{
			68,
			256,
			512,
			772,
			1024,
			1284,
			1540,
			1792,
			2056,
			2316,
			4112,
			4372,
			4628,
			4880,
			5140,
			5392,
			4096,
			4356,
			4612,
			4864,
			5124,
			5376,
			5632,
			5892,
			6156,
			6408,
			8240,
			8500,
			8756,
			9008,
			9268,
			9520,
			8224,
			8484,
			8740,
			8992,
			9252,
			9504,
			9760,
			10020,
			10284,
			10536,
			12340,
			12592,
			12848,
			13108,
			13360,
			13620,
			12324,
			12576,
			12832,
			13092,
			13344,
			13604,
			13860,
			14112,
			14376,
			14636,
			16400,
			16660,
			16916,
			17168,
			17428,
			17680,
			16384,
			16644,
			16900,
			17152,
			17412,
			17664,
			17920,
			18180,
			18444,
			18696,
			20500,
			20752,
			21008,
			21268,
			21520,
			21780,
			20484,
			20736,
			20992,
			21252,
			21504,
			21764,
			22020,
			22272,
			22536,
			22796,
			24628,
			24880,
			25136,
			25396,
			25648,
			25908,
			24612,
			24864,
			25120,
			25380,
			25632,
			25892,
			26148,
			26400,
			26664,
			26924,
			28720,
			28980,
			29236,
			29488,
			29748,
			30000,
			28704,
			28964,
			29220,
			29472,
			29732,
			29984,
			30240,
			30500,
			30764,
			31016,
			32912,
			33172,
			33428,
			33680,
			33940,
			34192,
			32896,
			33156,
			33412,
			33664,
			33924,
			34176,
			34432,
			34692,
			34956,
			35208,
			37012,
			37264,
			37520,
			37780,
			38032,
			38292,
			36996,
			37248,
			37504,
			37764,
			38016,
			38276,
			38532,
			38784,
			39048,
			39308,
			85,
			273,
			529,
			789,
			1041,
			1301,
			69,
			257,
			513,
			773,
			1025,
			1285,
			1541,
			1793,
			2057,
			2317,
			4113,
			4373,
			4629,
			4881,
			5141,
			5393,
			4097,
			4357,
			4613,
			4865,
			5125,
			5377,
			5633,
			5893,
			6157,
			6409,
			8241,
			8501,
			8757,
			9009,
			9269,
			9521,
			8225,
			8485,
			8741,
			8993,
			9253,
			9505,
			9761,
			10021,
			10285,
			10537,
			12341,
			12593,
			12849,
			13109,
			13361,
			13621,
			12325,
			12577,
			12833,
			13093,
			13345,
			13605,
			13861,
			14113,
			14377,
			14637,
			16401,
			16661,
			16917,
			17169,
			17429,
			17681,
			16385,
			16645,
			16901,
			17153,
			17413,
			17665,
			17921,
			18181,
			18445,
			18697,
			20501,
			20753,
			21009,
			21269,
			21521,
			21781,
			20485,
			20737,
			20993,
			21253,
			21505,
			21765,
			22021,
			22273,
			22537,
			22797,
			24629,
			24881,
			25137,
			25397,
			25649,
			25909,
			24613,
			24865,
			25121,
			25381,
			25633,
			25893,
			26149,
			26401,
			26665,
			26925,
			28721,
			28981,
			29237,
			29489,
			29749,
			30001,
			28705,
			28965,
			29221,
			29473,
			29733,
			29985,
			30241,
			30501,
			30765,
			31017,
			32913,
			33173,
			33429,
			33681,
			33941,
			34193,
			32897,
			33157,
			33413,
			33665,
			33925,
			34177,
			34433,
			34693,
			34957,
			35209,
			37013,
			37265,
			37521,
			37781,
			38033,
			38293,
			36997,
			37249,
			37505,
			37765,
			38017,
			38277,
			38533,
			38785,
			39049,
			39309,
			41141,
			41393,
			41649,
			41909,
			42161,
			42421,
			41125,
			41377,
			41633,
			41893,
			42145,
			42405,
			42661,
			42913,
			43177,
			43437,
			45233,
			45493,
			45749,
			46001,
			46261,
			46513,
			45217,
			45477,
			45733,
			45985,
			46245,
			46497,
			46753,
			47013,
			47277,
			47529,
			49301,
			49553,
			49809,
			50069,
			50321,
			50581,
			49285,
			49537,
			49793,
			50053,
			50305,
			50565,
			50821,
			51073,
			51337,
			51597,
			53393,
			53653,
			53909,
			54161,
			54421,
			54673,
			53377,
			53637,
			53893,
			54145,
			54405,
			54657,
			54913,
			55173,
			55437,
			55689,
			57521,
			57781,
			58037,
			58289,
			58549,
			58801,
			57505,
			57765,
			58021,
			58273,
			58533,
			58785,
			59041,
			59301,
			59565,
			59817,
			61621,
			61873,
			62129,
			62389,
			62641,
			62901,
			61605,
			61857,
			62113,
			62373,
			62625,
			62885,
			63141,
			63393,
			63657,
			63917,
			85,
			273,
			529,
			789,
			1041,
			1301,
			69,
			257,
			513,
			773,
			1025,
			1285,
			1541,
			1793,
			2057,
			2317,
			4113,
			4373,
			4629,
			4881,
			5141,
			5393,
			4097,
			4357,
			4613,
			4865,
			5125,
			5377,
			5633,
			5893,
			6157,
			6409,
			8241,
			8501,
			8757,
			9009,
			9269,
			9521,
			8225,
			8485,
			8741,
			8993,
			9253,
			9505,
			9761,
			10021,
			10285,
			10537,
			12341,
			12593,
			12849,
			13109,
			13361,
			13621,
			12325,
			12577,
			12833,
			13093,
			13345,
			13605,
			13861,
			14113,
			14377,
			14637,
			16401,
			16661,
			16917,
			17169,
			17429,
			17681,
			16385,
			16645,
			16901,
			17153,
			17413,
			17665,
			17921,
			18181,
			18445,
			18697,
			20501,
			20753,
			21009,
			21269,
			21521,
			21781,
			20485,
			20737,
			20993,
			21253,
			21505,
			21765,
			22021,
			22273,
			22537,
			22797,
			24629,
			24881,
			25137,
			25397,
			25649,
			25909,
			70,
			258,
			514,
			774,
			1026,
			1286,
			1542,
			1794,
			2058,
			2318,
			1026,
			1286,
			1542,
			1794,
			2058,
			2318,
			4098,
			4358,
			4614,
			4866,
			5126,
			5378,
			5634,
			5894,
			6158,
			6410,
			5126,
			5378,
			5634,
			5894,
			6158,
			6410,
			8226,
			8486,
			8742,
			8994,
			9254,
			9506,
			9762,
			10022,
			10286,
			10538,
			9254,
			9506,
			9762,
			10022,
			10286,
			10538,
			12326,
			12578,
			12834,
			13094,
			13346,
			13606,
			13862,
			14114,
			14378,
			14638,
			13346,
			13606,
			13862,
			14114,
			14378,
			14638,
			16386,
			16646,
			16902,
			17154,
			17414,
			17666,
			17922,
			18182,
			18446,
			18698,
			17414,
			17666,
			17922,
			18182,
			18446,
			18698,
			20486,
			20738,
			20994,
			21254,
			21506,
			21766,
			22022,
			22274,
			22538,
			22798,
			21506,
			21766,
			22022,
			22274,
			22538,
			22798,
			24614,
			24866,
			25122,
			25382,
			25634,
			25894,
			26150,
			26402,
			26666,
			26926,
			25634,
			25894,
			26150,
			26402,
			26666,
			26926,
			28706,
			28966,
			29222,
			29474,
			29734,
			29986,
			30242,
			30502,
			30766,
			31018,
			29734,
			29986,
			30242,
			30502,
			30766,
			31018,
			32898,
			33158,
			33414,
			33666,
			33926,
			34178,
			34434,
			34694,
			34958,
			35210,
			33926,
			34178,
			34434,
			34694,
			34958,
			35210,
			36998,
			37250,
			37506,
			37766,
			38018,
			38278,
			38534,
			38786,
			39050,
			39310,
			13347,
			13607,
			13863,
			14115,
			14379,
			14639,
			16387,
			16647,
			16903,
			17155,
			17415,
			17667,
			17923,
			18183,
			18447,
			18699,
			17415,
			17667,
			17923,
			18183,
			18447,
			18699,
			20487,
			20739,
			20995,
			21255,
			21507,
			21767,
			22023,
			22275,
			22539,
			22799,
			21507,
			21767,
			22023,
			22275,
			22539,
			22799,
			24615,
			24867,
			25123,
			25383,
			25635,
			25895,
			26151,
			26403,
			26667,
			26927,
			25635,
			25895,
			26151,
			26403,
			26667,
			26927,
			28707,
			28967,
			29223,
			29475,
			29735,
			29987,
			30243,
			30503,
			30767,
			31019,
			29735,
			29987,
			30243,
			30503,
			30767,
			31019,
			32899,
			33159,
			33415,
			33667,
			33927,
			34179,
			34435,
			34695,
			34959,
			35211,
			33927,
			34179,
			34435,
			34695,
			34959,
			35211,
			36999,
			37251,
			37507,
			37767,
			38019,
			38279,
			38535,
			38787,
			39051,
			39311,
			38019,
			38279,
			38535,
			38787,
			39051,
			39311,
			41127,
			41379,
			41635,
			41895,
			42147,
			42407,
			42663,
			42915,
			43179,
			43439,
			42147,
			42407,
			42663,
			42915,
			43179,
			43439,
			45219,
			45479,
			45735,
			45987,
			46247,
			46499,
			46755,
			47015,
			47279,
			47531,
			46247,
			46499,
			46755,
			47015,
			47279,
			47531,
			49287,
			49539,
			49795,
			50055,
			50307,
			50567,
			50823,
			51075,
			51339,
			51599,
			50307,
			50567,
			50823,
			51075,
			51339,
			51599,
			53379,
			53639,
			53895,
			54147,
			54407,
			54659,
			54915,
			55175,
			55439,
			55691,
			54407,
			54659,
			54915,
			55175,
			55439,
			55691,
			57507,
			57767,
			58023,
			58275,
			58535,
			58787,
			59043,
			59303,
			59567,
			59819,
			58535,
			58787,
			59043,
			59303,
			59567,
			59819,
			61607,
			61859,
			62115,
			62375,
			62627,
			62887,
			63143,
			63395,
			63659,
			63919,
			62627,
			62887,
			63143,
			63395,
			63659,
			63919,
			71,
			259,
			515,
			775,
			1027,
			1287,
			1543,
			1795,
			2059,
			2319,
			1027,
			1287,
			1543,
			1795,
			2059,
			2319,
			4099,
			4359,
			4615,
			4867,
			5127,
			5379,
			5635,
			5895,
			6159,
			6411,
			5127,
			5379,
			5635,
			5895,
			6159,
			6411,
			8227,
			8487,
			8743,
			8995,
			9255,
			9507,
			9763,
			10023,
			10287,
			10539,
			9255,
			9507,
			9763,
			10023,
			10287,
			10539,
			12327,
			12579,
			12835,
			13095,
			13347,
			13607,
			13863,
			14115,
			14379,
			14639,
			13347,
			13607,
			13863,
			14115,
			14379,
			14639,
			16387,
			16647,
			16903,
			17155,
			17415,
			17667,
			17923,
			18183,
			18447,
			18699,
			17415,
			17667,
			17923,
			18183,
			18447,
			18699,
			20487,
			20739,
			20995,
			21255,
			21507,
			21767,
			22023,
			22275,
			22539,
			22799,
			21507,
			21767,
			22023,
			22275,
			22539,
			22799,
			24615,
			24867,
			25123,
			25383,
			25635,
			25895,
			26151,
			26403,
			26667,
			26927,
			25635,
			25895,
			26151,
			26403,
			26667,
			26927,
			28707,
			28967,
			29223,
			29475,
			29735,
			29987,
			30243,
			30503,
			30767,
			31019,
			29735,
			29987,
			30243,
			30503,
			30767,
			31019,
			32899,
			33159,
			33415,
			33667,
			33927,
			34179,
			34435,
			34695,
			34959,
			35211,
			33927,
			34179,
			34435,
			34695,
			34959,
			35211,
			36999,
			37251,
			37507,
			37767,
			38019,
			38279,
			38535,
			38787,
			39051,
			39311,
			38019,
			38279,
			38535,
			38787,
			39051,
			39311,
			1540,
			1792,
			2056,
			2316,
			2572,
			2824,
			3084,
			3336,
			3592,
			3852,
			4112,
			4372,
			4628,
			4880,
			5140,
			5392,
			5632,
			5892,
			6156,
			6408,
			6664,
			6924,
			7176,
			7436,
			7692,
			7944,
			8240,
			8500,
			8756,
			9008,
			9268,
			9520,
			9760,
			10020,
			10284,
			10536,
			10792,
			11052,
			11304,
			11564,
			11820,
			12072,
			12340,
			12592,
			12848,
			13108,
			13360,
			13620,
			13860,
			14112,
			14376,
			14636,
			14892,
			15144,
			15404,
			15656,
			15912,
			16172,
			16400,
			16660,
			16916,
			17168,
			17428,
			17680,
			17920,
			18180,
			18444,
			18696,
			18952,
			19212,
			19464,
			19724,
			19980,
			20232,
			20500,
			20752,
			21008,
			21268,
			21520,
			21780,
			22020,
			22272,
			22536,
			22796,
			23052,
			23304,
			23564,
			23816,
			24072,
			24332,
			24628,
			24880,
			25136,
			25396,
			25648,
			25908,
			26148,
			26400,
			26664,
			26924,
			27180,
			27432,
			27692,
			27944,
			28200,
			28460,
			28720,
			28980,
			29236,
			29488,
			29748,
			30000,
			30240,
			30500,
			30764,
			31016,
			31272,
			31532,
			31784,
			32044,
			32300,
			32552,
			32912,
			33172,
			33428,
			33680,
			33940,
			34192,
			34432,
			34692,
			34956,
			35208,
			35464,
			35724,
			35976,
			36236,
			36492,
			36744,
			37012,
			37264,
			37520,
			37780,
			38032,
			38292,
			38532,
			38784,
			39048,
			39308,
			39564,
			39816,
			40076,
			40328,
			40584,
			40844,
			85,
			273,
			529,
			789,
			1041,
			1301,
			1541,
			1793,
			2057,
			2317,
			2573,
			2825,
			3085,
			3337,
			3593,
			3853,
			4113,
			4373,
			4629,
			4881,
			5141,
			5393,
			5633,
			5893,
			6157,
			6409,
			6665,
			6925,
			7177,
			7437,
			7693,
			7945,
			8241,
			8501,
			8757,
			9009,
			9269,
			9521,
			9761,
			10021,
			10285,
			10537,
			10793,
			11053,
			11305,
			11565,
			11821,
			12073,
			12341,
			12593,
			12849,
			13109,
			13361,
			13621,
			13861,
			14113,
			14377,
			14637,
			14893,
			15145,
			15405,
			15657,
			15913,
			16173,
			16401,
			16661,
			16917,
			17169,
			17429,
			17681,
			17921,
			18181,
			18445,
			18697,
			18953,
			19213,
			19465,
			19725,
			19981,
			20233,
			20501,
			20753,
			21009,
			21269,
			21521,
			21781,
			22021,
			22273,
			22537,
			22797,
			23053,
			23305,
			23565,
			23817,
			24073,
			24333,
			24629,
			24881,
			25137,
			25397,
			25649,
			25909,
			26149,
			26401,
			26665,
			26925,
			27181,
			27433,
			27693,
			27945,
			28201,
			28461,
			28721,
			28981,
			29237,
			29489,
			29749,
			30001,
			30241,
			30501,
			30765,
			31017,
			31273,
			31533,
			31785,
			32045,
			32301,
			32553,
			32913,
			33173,
			33429,
			33681,
			33941,
			34193,
			34433,
			34693,
			34957,
			35209,
			35465,
			35725,
			35977,
			36237,
			36493,
			36745,
			37013,
			37265,
			37521,
			37781,
			38033,
			38293,
			38533,
			38785,
			39049,
			39309,
			39565,
			39817,
			40077,
			40329,
			40585,
			40845,
			41141,
			41393,
			41649,
			41909,
			42161,
			42421,
			42661,
			42913,
			43177,
			43437,
			43693,
			43945,
			44205,
			44457,
			44713,
			44973,
			45233,
			45493,
			45749,
			46001,
			46261,
			46513,
			46753,
			47013,
			47277,
			47529,
			47785,
			48045,
			48297,
			48557,
			48813,
			49065,
			49301,
			49553,
			49809,
			50069,
			50321,
			50581,
			50821,
			51073,
			51337,
			51597,
			51853,
			52105,
			52365,
			52617,
			52873,
			53133,
			53393,
			53653,
			53909,
			54161,
			54421,
			54673,
			54913,
			55173,
			55437,
			55689,
			55945,
			56205,
			56457,
			56717,
			56973,
			57225,
			57521,
			57781,
			58037,
			58289,
			58549,
			58801,
			59041,
			59301,
			59565,
			59817,
			60073,
			60333,
			60585,
			60845,
			61101,
			61353,
			61621,
			61873,
			62129,
			62389,
			62641,
			62901,
			63141,
			63393,
			63657,
			63917,
			64173,
			64425,
			64685,
			64937,
			65193,
			65453,
			85,
			273,
			529,
			789,
			1041,
			1301,
			1541,
			1793,
			2057,
			2317,
			2573,
			2825,
			3085,
			3337,
			3593,
			3853,
			4113,
			4373,
			4629,
			4881,
			5141,
			5393,
			5633,
			5893,
			6157,
			6409,
			6665,
			6925,
			7177,
			7437,
			7693,
			7945,
			8241,
			8501,
			8757,
			9009,
			9269,
			9521,
			9761,
			10021,
			10285,
			10537,
			10793,
			11053,
			11305,
			11565,
			11821,
			12073,
			12341,
			12593,
			12849,
			13109,
			13361,
			13621,
			13861,
			14113,
			14377,
			14637,
			14893,
			15145,
			15405,
			15657,
			15913,
			16173,
			16401,
			16661,
			16917,
			17169,
			17429,
			17681,
			17921,
			18181,
			18445,
			18697,
			18953,
			19213,
			19465,
			19725,
			19981,
			20233,
			20501,
			20753,
			21009,
			21269,
			21521,
			21781,
			22021,
			22273,
			22537,
			22797,
			23053,
			23305,
			23565,
			23817,
			24073,
			24333,
			24629,
			24881,
			25137,
			25397,
			25649,
			25909,
			64190,
			64442,
			64702,
			64954,
			65210,
			65470,
			70,
			258,
			514,
			774,
			1026,
			1286,
			1542,
			1794,
			2058,
			2318,
			2590,
			2842,
			3102,
			3354,
			3610,
			3870,
			4098,
			4358,
			4614,
			4866,
			5126,
			5378,
			5634,
			5894,
			6158,
			6410,
			6682,
			6942,
			7194,
			7454,
			7710,
			7962,
			8226,
			8486,
			8742,
			8994,
			9254,
			9506,
			9762,
			10022,
			10286,
			10538,
			10810,
			11070,
			11322,
			11582,
			11838,
			12090,
			12326,
			12578,
			12834,
			13094,
			13346,
			13606,
			13862,
			14114,
			14378,
			14638,
			14910,
			15162,
			15422,
			15674,
			15930,
			16190,
			16386,
			16646,
			16902,
			17154,
			17414,
			17666,
			17922,
			18182,
			18446,
			18698,
			18970,
			19230,
			19482,
			19742,
			19998,
			20250,
			20486,
			20738,
			20994,
			21254,
			21506,
			21766,
			22022,
			22274,
			22538,
			22798,
			23070,
			23322,
			23582,
			23834,
			24090,
			24350,
			24614,
			24866,
			25122,
			25382,
			25634,
			25894,
			26150,
			26402,
			26666,
			26926,
			27198,
			27450,
			27710,
			27962,
			28218,
			28478,
			28706,
			28966,
			29222,
			29474,
			29734,
			29986,
			30242,
			30502,
			30766,
			31018,
			31290,
			31550,
			31802,
			32062,
			32318,
			32570,
			32898,
			33158,
			33414,
			33666,
			33926,
			34178,
			34434,
			34694,
			34958,
			35210,
			35482,
			35742,
			35994,
			36254,
			36510,
			36762,
			36998,
			37250,
			37506,
			37766,
			13347,
			13607,
			13863,
			14115,
			14379,
			14639,
			14911,
			15163,
			15423,
			15675,
			15931,
			16191,
			16387,
			16647,
			16903,
			17155,
			17415,
			17667,
			17923,
			18183,
			18447,
			18699,
			18971,
			19231,
			19483,
			19743,
			19999,
			20251,
			20487,
			20739,
			20995,
			21255,
			21507,
			21767,
			22023,
			22275,
			22539,
			22799,
			23071,
			23323,
			23583,
			23835,
			24091,
			24351,
			24615,
			24867,
			25123,
			25383,
			25635,
			25895,
			26151,
			26403,
			26667,
			26927,
			27199,
			27451,
			27711,
			27963,
			28219,
			28479,
			28707,
			28967,
			29223,
			29475,
			29735,
			29987,
			30243,
			30503,
			30767,
			31019,
			31291,
			31551,
			31803,
			32063,
			32319,
			32571,
			32899,
			33159,
			33415,
			33667,
			33927,
			34179,
			34435,
			34695,
			34959,
			35211,
			35483,
			35743,
			35995,
			36255,
			36511,
			36763,
			36999,
			37251,
			37507,
			37767,
			38019,
			38279,
			38535,
			38787,
			39051,
			39311,
			39583,
			39835,
			40095,
			40347,
			40603,
			40863,
			41127,
			41379,
			41635,
			41895,
			42147,
			42407,
			42663,
			42915,
			43179,
			43439,
			43711,
			43963,
			44223,
			44475,
			44731,
			44991,
			45219,
			45479,
			45735,
			45987,
			46247,
			46499,
			46755,
			47015,
			47279,
			47531,
			47803,
			48063,
			48315,
			48575,
			48831,
			49083,
			49287,
			49539,
			49795,
			50055,
			50307,
			50567,
			50823,
			51075,
			51339,
			51599,
			51871,
			52123,
			52383,
			52635,
			52891,
			53151,
			53379,
			53639,
			53895,
			54147,
			54407,
			54659,
			54915,
			55175,
			55439,
			55691,
			55963,
			56223,
			56475,
			56735,
			56991,
			57243,
			57507,
			57767,
			58023,
			58275,
			58535,
			58787,
			59043,
			59303,
			59567,
			59819,
			60091,
			60351,
			60603,
			60863,
			61119,
			61371,
			61607,
			61859,
			62115,
			62375,
			62627,
			62887,
			63143,
			63395,
			63659,
			63919,
			64191,
			64443,
			64703,
			64955,
			65211,
			65471,
			71,
			259,
			515,
			775,
			1027,
			1287,
			1543,
			1795,
			2059,
			2319,
			2591,
			2843,
			3103,
			3355,
			3611,
			3871,
			4099,
			4359,
			4615,
			4867,
			5127,
			5379,
			5635,
			5895,
			6159,
			6411,
			6683,
			6943,
			7195,
			7455,
			7711,
			7963,
			8227,
			8487,
			8743,
			8995,
			9255,
			9507,
			9763,
			10023,
			10287,
			10539,
			10811,
			11071,
			11323,
			11583,
			11839,
			12091,
			12327,
			12579,
			12835,
			13095,
			13347,
			13607,
			13863,
			14115,
			14379,
			14639,
			14911,
			15163,
			15423,
			15675,
			15931,
			16191,
			16387,
			16647,
			16903,
			17155,
			17415,
			17667,
			17923,
			18183,
			18447,
			18699,
			18971,
			19231,
			19483,
			19743,
			19999,
			20251,
			20487,
			20739,
			20995,
			21255,
			21507,
			21767,
			22023,
			22275,
			22539,
			22799,
			23071,
			23323,
			23583,
			23835,
			24091,
			24351,
			24615,
			24867,
			25123,
			25383,
			25635,
			25895,
			26151,
			26403,
			26667,
			26927,
			27199,
			27451,
			27711,
			27963,
			28219,
			28479,
			28707,
			28967,
			29223,
			29475,
			29735,
			29987,
			30243,
			30503,
			30767,
			31019,
			31291,
			31551,
			31803,
			32063,
			32319,
			32571,
			32899,
			33159,
			33415,
			33667,
			33927,
			34179,
			34435,
			34695,
			34959,
			35211,
			35483,
			35743,
			35995,
			36255,
			36511,
			36763,
			36999,
			37251,
			37507,
			37767,
			38019,
			38279,
			38535,
			38787,
			39051,
			39311
		};

		private static byte[] rl0 = new byte[]
		{
			68,
			0,
			0,
			4,
			8,
			12,
			12,
			8,
			0,
			4,
			4,
			0,
			12,
			8,
			8,
			12,
			32,
			36,
			36,
			32,
			44,
			40,
			40,
			44,
			36,
			32,
			32,
			36,
			40,
			44,
			44,
			40,
			0,
			4,
			4,
			0,
			12,
			8,
			8,
			12,
			4,
			0,
			0,
			4,
			8,
			12,
			12,
			8,
			36,
			32,
			32,
			36,
			40,
			44,
			44,
			40,
			32,
			36,
			36,
			32,
			44,
			40,
			40,
			44,
			128,
			132,
			132,
			128,
			140,
			136,
			136,
			140,
			132,
			128,
			128,
			132,
			136,
			140,
			140,
			136,
			164,
			160,
			160,
			164,
			168,
			172,
			172,
			168,
			160,
			164,
			164,
			160,
			172,
			168,
			168,
			172,
			132,
			128,
			128,
			132,
			136,
			140,
			140,
			136,
			128,
			132,
			132,
			128,
			140,
			136,
			136,
			140,
			160,
			164,
			164,
			160,
			172,
			168,
			168,
			172,
			164,
			160,
			160,
			164,
			168,
			172,
			172,
			168,
			69,
			1,
			1,
			5,
			9,
			13,
			13,
			9,
			1,
			5,
			5,
			1,
			13,
			9,
			9,
			13,
			33,
			37,
			37,
			33,
			45,
			41,
			41,
			45,
			37,
			33,
			33,
			37,
			41,
			45,
			45,
			41,
			1,
			5,
			5,
			1,
			13,
			9,
			9,
			13,
			5,
			1,
			1,
			5,
			9,
			13,
			13,
			9,
			37,
			33,
			33,
			37,
			41,
			45,
			45,
			41,
			33,
			37,
			37,
			33,
			45,
			41,
			41,
			45,
			129,
			133,
			133,
			129,
			141,
			137,
			137,
			141,
			133,
			129,
			129,
			133,
			137,
			141,
			141,
			137,
			165,
			161,
			161,
			165,
			169,
			173,
			173,
			169,
			161,
			165,
			165,
			161,
			173,
			169,
			169,
			173,
			133,
			129,
			129,
			133,
			137,
			141,
			141,
			137,
			129,
			133,
			133,
			129,
			141,
			137,
			137,
			141,
			161,
			165,
			165,
			161,
			173,
			169,
			169,
			173,
			165,
			161,
			161,
			165,
			169,
			173,
			173,
			169
		};

		private static byte[] rl1 = new byte[]
		{
			0,
			4,
			4,
			0,
			12,
			8,
			8,
			12,
			4,
			0,
			0,
			4,
			8,
			12,
			12,
			8,
			36,
			32,
			32,
			36,
			40,
			44,
			44,
			40,
			32,
			36,
			36,
			32,
			44,
			40,
			40,
			44,
			4,
			0,
			0,
			4,
			8,
			12,
			12,
			8,
			0,
			4,
			4,
			0,
			12,
			8,
			8,
			12,
			32,
			36,
			36,
			32,
			44,
			40,
			40,
			44,
			36,
			32,
			32,
			36,
			40,
			44,
			44,
			40,
			132,
			128,
			128,
			132,
			136,
			140,
			140,
			136,
			128,
			132,
			132,
			128,
			140,
			136,
			136,
			140,
			160,
			164,
			164,
			160,
			172,
			168,
			168,
			172,
			164,
			160,
			160,
			164,
			168,
			172,
			172,
			168,
			128,
			132,
			132,
			128,
			140,
			136,
			136,
			140,
			132,
			128,
			128,
			132,
			136,
			140,
			140,
			136,
			164,
			160,
			160,
			164,
			168,
			172,
			172,
			168,
			160,
			164,
			164,
			160,
			172,
			168,
			168,
			172,
			1,
			5,
			5,
			1,
			13,
			9,
			9,
			13,
			5,
			1,
			1,
			5,
			9,
			13,
			13,
			9,
			37,
			33,
			33,
			37,
			41,
			45,
			45,
			41,
			33,
			37,
			37,
			33,
			45,
			41,
			41,
			45,
			5,
			1,
			1,
			5,
			9,
			13,
			13,
			9,
			1,
			5,
			5,
			1,
			13,
			9,
			9,
			13,
			33,
			37,
			37,
			33,
			45,
			41,
			41,
			45,
			37,
			33,
			33,
			37,
			41,
			45,
			45,
			41,
			133,
			129,
			129,
			133,
			137,
			141,
			141,
			137,
			129,
			133,
			133,
			129,
			141,
			137,
			137,
			141,
			161,
			165,
			165,
			161,
			173,
			169,
			169,
			173,
			165,
			161,
			161,
			165,
			169,
			173,
			173,
			169,
			129,
			133,
			133,
			129,
			141,
			137,
			137,
			141,
			133,
			129,
			129,
			133,
			137,
			141,
			141,
			137,
			165,
			161,
			161,
			165,
			169,
			173,
			173,
			169,
			161,
			165,
			165,
			161,
			173,
			169,
			169,
			173
		};

		private static byte[] rr0 = new byte[]
		{
			68,
			69,
			0,
			1,
			0,
			1,
			4,
			5,
			0,
			1,
			4,
			5,
			4,
			5,
			0,
			1,
			8,
			9,
			12,
			13,
			12,
			13,
			8,
			9,
			12,
			13,
			8,
			9,
			8,
			9,
			12,
			13,
			0,
			1,
			4,
			5,
			4,
			5,
			0,
			1,
			4,
			5,
			0,
			1,
			0,
			1,
			4,
			5,
			12,
			13,
			8,
			9,
			8,
			9,
			12,
			13,
			8,
			9,
			12,
			13,
			12,
			13,
			8,
			9,
			32,
			33,
			36,
			37,
			36,
			37,
			32,
			33,
			36,
			37,
			32,
			33,
			32,
			33,
			36,
			37,
			44,
			45,
			40,
			41,
			40,
			41,
			44,
			45,
			40,
			41,
			44,
			45,
			44,
			45,
			40,
			41,
			36,
			37,
			32,
			33,
			32,
			33,
			36,
			37,
			32,
			33,
			36,
			37,
			36,
			37,
			32,
			33,
			40,
			41,
			44,
			45,
			44,
			45,
			40,
			41,
			44,
			45,
			40,
			41,
			40,
			41,
			44,
			45,
			0,
			1,
			4,
			5,
			4,
			5,
			0,
			1,
			4,
			5,
			0,
			1,
			0,
			1,
			4,
			5,
			12,
			13,
			8,
			9,
			8,
			9,
			12,
			13,
			8,
			9,
			12,
			13,
			12,
			13,
			8,
			9,
			4,
			5,
			0,
			1,
			0,
			1,
			4,
			5,
			0,
			1,
			4,
			5,
			4,
			5,
			0,
			1,
			8,
			9,
			12,
			13,
			12,
			13,
			8,
			9,
			12,
			13,
			8,
			9,
			8,
			9,
			12,
			13,
			36,
			37,
			32,
			33,
			32,
			33,
			36,
			37,
			32,
			33,
			36,
			37,
			36,
			37,
			32,
			33,
			40,
			41,
			44,
			45,
			44,
			45,
			40,
			41,
			44,
			45,
			40,
			41,
			40,
			41,
			44,
			45,
			32,
			33,
			36,
			37,
			36,
			37,
			32,
			33,
			36,
			37,
			32,
			33,
			32,
			33,
			36,
			37,
			44,
			45,
			40,
			41,
			40,
			41,
			44,
			45,
			40,
			41,
			44,
			45,
			44,
			45,
			40,
			41
		};

		private static byte[] rr1 = new byte[]
		{
			128,
			129,
			132,
			133,
			132,
			133,
			128,
			129,
			132,
			133,
			128,
			129,
			128,
			129,
			132,
			133,
			140,
			141,
			136,
			137,
			136,
			137,
			140,
			141,
			136,
			137,
			140,
			141,
			140,
			141,
			136,
			137,
			132,
			133,
			128,
			129,
			128,
			129,
			132,
			133,
			128,
			129,
			132,
			133,
			132,
			133,
			128,
			129,
			136,
			137,
			140,
			141,
			140,
			141,
			136,
			137,
			140,
			141,
			136,
			137,
			136,
			137,
			140,
			141,
			164,
			165,
			160,
			161,
			160,
			161,
			164,
			165,
			160,
			161,
			164,
			165,
			164,
			165,
			160,
			161,
			168,
			169,
			172,
			173,
			172,
			173,
			168,
			169,
			172,
			173,
			168,
			169,
			168,
			169,
			172,
			173,
			160,
			161,
			164,
			165,
			164,
			165,
			160,
			161,
			164,
			165,
			160,
			161,
			160,
			161,
			164,
			165,
			172,
			173,
			168,
			169,
			168,
			169,
			172,
			173,
			168,
			169,
			172,
			173,
			172,
			173,
			168,
			169,
			132,
			133,
			128,
			129,
			128,
			129,
			132,
			133,
			128,
			129,
			132,
			133,
			132,
			133,
			128,
			129,
			136,
			137,
			140,
			141,
			140,
			141,
			136,
			137,
			140,
			141,
			136,
			137,
			136,
			137,
			140,
			141,
			128,
			129,
			132,
			133,
			132,
			133,
			128,
			129,
			132,
			133,
			128,
			129,
			128,
			129,
			132,
			133,
			140,
			141,
			136,
			137,
			136,
			137,
			140,
			141,
			136,
			137,
			140,
			141,
			140,
			141,
			136,
			137,
			160,
			161,
			164,
			165,
			164,
			165,
			160,
			161,
			164,
			165,
			160,
			161,
			160,
			161,
			164,
			165,
			172,
			173,
			168,
			169,
			168,
			169,
			172,
			173,
			168,
			169,
			172,
			173,
			172,
			173,
			168,
			169,
			164,
			165,
			160,
			161,
			160,
			161,
			164,
			165,
			160,
			161,
			164,
			165,
			164,
			165,
			160,
			161,
			168,
			169,
			172,
			173,
			172,
			173,
			168,
			169,
			172,
			173,
			168,
			169,
			168,
			169,
			172,
			173
		};

		private static byte[] sraf = new byte[]
		{
			68,
			69,
			0,
			1,
			0,
			1,
			4,
			5,
			0,
			1,
			4,
			5,
			4,
			5,
			0,
			1,
			8,
			9,
			12,
			13,
			12,
			13,
			8,
			9,
			12,
			13,
			8,
			9,
			8,
			9,
			12,
			13,
			0,
			1,
			4,
			5,
			4,
			5,
			0,
			1,
			4,
			5,
			0,
			1,
			0,
			1,
			4,
			5,
			12,
			13,
			8,
			9,
			8,
			9,
			12,
			13,
			8,
			9,
			12,
			13,
			12,
			13,
			8,
			9,
			32,
			33,
			36,
			37,
			36,
			37,
			32,
			33,
			36,
			37,
			32,
			33,
			32,
			33,
			36,
			37,
			44,
			45,
			40,
			41,
			40,
			41,
			44,
			45,
			40,
			41,
			44,
			45,
			44,
			45,
			40,
			41,
			36,
			37,
			32,
			33,
			32,
			33,
			36,
			37,
			32,
			33,
			36,
			37,
			36,
			37,
			32,
			33,
			40,
			41,
			44,
			45,
			44,
			45,
			40,
			41,
			44,
			45,
			40,
			41,
			40,
			41,
			44,
			45,
			132,
			133,
			128,
			129,
			128,
			129,
			132,
			133,
			128,
			129,
			132,
			133,
			132,
			133,
			128,
			129,
			136,
			137,
			140,
			141,
			140,
			141,
			136,
			137,
			140,
			141,
			136,
			137,
			136,
			137,
			140,
			141,
			128,
			129,
			132,
			133,
			132,
			133,
			128,
			129,
			132,
			133,
			128,
			129,
			128,
			129,
			132,
			133,
			140,
			141,
			136,
			137,
			136,
			137,
			140,
			141,
			136,
			137,
			140,
			141,
			140,
			141,
			136,
			137,
			160,
			161,
			164,
			165,
			164,
			165,
			160,
			161,
			164,
			165,
			160,
			161,
			160,
			161,
			164,
			165,
			172,
			173,
			168,
			169,
			168,
			169,
			172,
			173,
			168,
			169,
			172,
			173,
			172,
			173,
			168,
			169,
			164,
			165,
			160,
			161,
			160,
			161,
			164,
			165,
			160,
			161,
			164,
			165,
			164,
			165,
			160,
			161,
			168,
			169,
			172,
			173,
			172,
			173,
			168,
			169,
			172,
			173,
			168,
			169,
			168,
			169,
			172,
			173
		};

		private delegate void XFXOPDO(byte cmd);

		private delegate void FXCBOPDO(byte cmd, ushort adr);

		private delegate void ALUALGORITHM(byte src);

		public delegate byte MEMREADER(ushort ADDR);
	}
}

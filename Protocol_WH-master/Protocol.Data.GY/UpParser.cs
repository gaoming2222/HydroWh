using Protocol.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hydrology.Entity;

//namespace Protocol.Data.GY
//{
//    class UpParser : IUp
//    {
//        public bool Parse(string msg, out CReportStruct report)
//        {
//            throw new NotImplementedException();
//        }

//        public bool Parse_1(string msg, out CReportStruct report)
//        {
//            throw new NotImplementedException();
//        }

//        public bool Parse_2(string msg, out CReportStruct report)
//        {
//            throw new NotImplementedException();
//        }

//        public bool Parse_beidou(string sid, EMessageType type, string msg, out CReportStruct upReport)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}


namespace Protocol.Data.GY
{
    public class UpParser : IUp
    {
        /// <summary>
        /// 规约协议上行数据解析
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public bool Parse(string msg, out CReportStruct report)
        {
            report = null;
            try
            {
                if (msg == "")
                {
                    return false;
                }
                // 数据格式
                ////7E7E 已经在通讯协议刨去了
                //00
                //0060405210
                //04D2
                //31
                //0045
                //02
                //534E
                //180410000000
                //F1
                //28
                //0060405210
                //48
                //F0
                //28
                //1804092300
                //04
                //18
                //000005
                //26
                //19
                //041130 041130 041130 041130 041130 041130 041130 041130 041130 041130 041130 041130 041130 
                //03
                //0A13
                /// 数据格式结束

                string data = msg;
                // 2018-12-11 gaoming 添加
                string funcCode = data.Substring(20, 2);
                //丢弃包头 (4位)
                //data = data.Substring(4);
                //丢弃中心站地址（2位）
                data = data.Substring(2);
                //遥测站地址（10位）00+8位站号
                string id = data.Substring(2, 8);
                data = data.Substring(10);
                //丢弃密码（4位）
                data = data.Substring(4);
                //功能码即报类（2位） 31、32、33、34 分别是均匀时段报、定时报、加报、小时报
                string type = data.Substring(0, 2);
                data = data.Substring(2);
                //报文上下文标识及长度（4位）
                string context = data.Substring(0, 1);
                string contextLengthString = data.Substring(1, 3);
                if (context != "0")
                {
                    // 不是上行报文
                    return false;
                }
                // 报文正文长度 16进制转10进制（字节数）*2
                int contextLength = (int.Parse(contextLengthString, System.Globalization.NumberStyles.AllowHexSpecifier));
                data = data.Substring(4);
                // 丢弃起始符 02
                data = data.Substring(1);

                // 报文数据
                string message = data.Substring(0, contextLength);
                data = data.Substring(contextLength);
                bool result = DealData(message, type, out report);

                report.Stationid = id;
                report.Type = "1G";
                report.RecvTime = DateTime.Now;
                switch (type)
                {
                    case "2F":
                        break; //链路维持报
                    case "30": //测试报
                        report.ReportType = EMessageType.ETest;
                        break;
                    case "31":
                        report.ReportType = EMessageType.EUinform;
                        break;
                    case "32":
                        report.ReportType = EMessageType.ETimed;
                        break;
                    case "33":
                        report.ReportType = EMessageType.EAdditional;
                        break;
                    case "34":
                        report.ReportType = EMessageType.EHour;
                        break;
                }

                // 丢弃结束符 03
                data = data.Substring(2);
                // 丢弃校验
                data = data.Substring(4);

                return result;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("数据：" + msg);
                System.Diagnostics.Debug.WriteLine("规约协议解析不完整" + e.Message);
            }
            return false;
        }
        #region 帮助函数
        /// <summary>
        /// CRC16校验  byte[] 转b byte[];
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] CRC16(byte[] data)
        {
            byte[] returnVal = new byte[2];
            byte CRC16Lo, CRC16Hi, CL, CH, SaveHi, SaveLo;
            int i, Flag;
            CRC16Lo = 0xFF;
            CRC16Hi = 0xFF;
            CL = 0x86;
            CH = 0x68;
            for (i = 0; i < data.Length; i++)
            {
                CRC16Lo = (byte)(CRC16Lo ^ data[i]);//每一个数据与CRC寄存器进行异或
                for (Flag = 0; Flag <= 7; Flag++)
                {
                    SaveHi = CRC16Hi;
                    SaveLo = CRC16Lo;
                    CRC16Hi = (byte)(CRC16Hi >> 1);//高位右移一位
                    CRC16Lo = (byte)(CRC16Lo >> 1);//低位右移一位
                    if ((SaveHi & 0x01) == 0x01)//如果高位字节最后一位为
                    {
                        CRC16Lo = (byte)(CRC16Lo | 0x80);//则低位字节右移后前面补 否则自动补0
                    }
                    if ((SaveLo & 0x01) == 0x01)//如果LSB为1，则与多项式码进行异或
                    {
                        CRC16Hi = (byte)(CRC16Hi ^ CH);
                        CRC16Lo = (byte)(CRC16Lo ^ CL);
                    }
                }
            }
            returnVal[0] = CRC16Hi;//CRC高位
            returnVal[1] = CRC16Lo;//CRC低位
            return returnVal;
        }

        #endregion


        public bool Parse_beidou(string sid, EMessageType type, string msg, out CReportStruct upReport)
        {
            throw new NotImplementedException();
        }

        public bool DealData(string msg, string reportType, out CReportStruct report)
        {
            report = new CReportStruct();
            try
            {
                string data = msg;
                // 丢弃流水号（4位）
                data = data.Substring(4);
                // 丢弃发报时间（12位）
                DateTime sendTime = new DateTime(
                    year: int.Parse("20" + data.Substring(0, 2)),
                    month: int.Parse(data.Substring(2, 2)),
                    day: int.Parse(data.Substring(4, 2)),
                    hour: int.Parse(data.Substring(6, 2)),
                    minute: int.Parse(data.Substring(8, 2)),
                    second: int.Parse(data.Substring(10, 2))
                    );
                data = data.Substring(12);

                // report中的数据初始化
                List<CReportData> datas = new List<CReportData>();
                TimeSpan span = new TimeSpan(0);
                EStationType type = new EStationType();
                DateTime dataTime = sendTime;
                int dataAccuracyLength;
                int dataLength;
                Decimal dataAccuracy;
                Decimal dayRain;
                Decimal diffRain;
                string dataDefine, dataDefine1, dataDefine2;

                while (data.Length >= 2)
                {
                    // 截取要素标识符
                    string sign = data.Substring(0, 2);
                    // 根据要素标识符取数据
                    switch (sign)
                    {
                        case "ST":
                            // 丢弃标识符
                            data = data.Substring(2);
                            // 丢弃一个字节+观测站地址
                            data = data.Substring(12);
                            // 遥测站分类码，不确定是不是一定在这个后面
                            string stationTypeString = data.Substring(0, 2);
                            data = data.Substring(2);
                            type = stationTypeString == "50" ? EStationType.ERainFall : EStationType.EHydrology;
                            break;
                        case "TT":
                            // 丢弃标识符
                            data = data.Substring(2);
                            //丢弃一个字节
                            data = data.Substring(2);
                            dataTime = new DateTime(
                                year: int.Parse("20" + data.Substring(0, 2)),
                                month: int.Parse(data.Substring(2, 2)),
                                day: int.Parse(data.Substring(4, 2)),
                                hour: int.Parse(data.Substring(6, 2)),
                                minute: int.Parse(data.Substring(8, 2)),
                                second: 0
                            );
                            data = data.Substring(10);
                            break;
                        case "DRxnn":
                            // 丢弃标识符
                            data = data.Substring(2);
                            // 丢弃数据定义18 3个字节、精度0
                            data = data.Substring(2);
                            // 时间步长
                            string timeSpanString = data.Substring(0, 6);
                            data = data.Substring(6);
                            TimeSpan timeSpan = new TimeSpan(Int32.Parse(timeSpanString.Substring(0, 2)), Int32.Parse(timeSpanString.Substring(2, 2)), Int32.Parse(timeSpanString.Substring(4, 2)), 0);
                            span = span + timeSpan;
                            break;
                        case "PD":
                            // 丢弃标识符
                            data = data.Substring(2);
                            // 丢弃数据定义19 3个字节、精度1
                            dataDefine1 = Convert.ToString(int.Parse(data.Substring(0, 1)), 2);
                            dataDefine2 = Convert.ToString(int.Parse(data.Substring(1, 1)), 2);
                            dataDefine = dataDefine1.PadLeft(4, '0') + dataDefine2.PadLeft(4, '0');
                            dataLength = Convert.ToInt32(dataDefine.Substring(0, 5), 2) * 2;
                            dataAccuracyLength = Convert.ToInt32(dataDefine.Substring(5, 3), 2);
                            dataAccuracy = 1;
                            while (dataAccuracyLength > 0)
                            {
                                dataAccuracy *= (decimal)0.1;
                                dataAccuracyLength--;
                            }
                            data = data.Substring(2);

                            string dayRainString = data.Substring(0, dataLength);
                            data = data.Substring(dataLength);
                            try
                            {
                                dayRain = Decimal.Parse(dayRainString) * dataAccuracy;
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine("规约协议数据截取错误" + e.ToString());
                            }
                            break;
                        case "PJ":
                            // 丢弃标识符
                            data = data.Substring(2);
                            // 丢弃数据定义19 3个字节、精度1
                            dataDefine1 = Convert.ToString(int.Parse(data.Substring(0, 1)), 2);
                            dataDefine2 = Convert.ToString(int.Parse(data.Substring(1, 1)), 2);
                            dataDefine = dataDefine1.PadLeft(4, '0') + dataDefine2.PadLeft(4, '0');
                            dataLength = Convert.ToInt32(dataDefine.Substring(0, 5), 2) * 2;
                            dataAccuracyLength = Convert.ToInt32(dataDefine.Substring(5, 3), 2);
                            dataAccuracy = 1;
                            while (dataAccuracyLength > 0)
                            {
                                dataAccuracy *= (decimal)0.1;
                                dataAccuracyLength--;
                            }
                            data = data.Substring(2);

                            string diffRainString = data.Substring(0, dataLength);
                            data = data.Substring(dataLength);
                            try
                            {
                                diffRain = Decimal.Parse(diffRainString) * dataAccuracy;
                            }
                            catch (Exception e)
                            {
                                System.Diagnostics.Debug.WriteLine("规约协议数据截取错误" + e.ToString());
                            }
                            break;
                        case "PT":
                            // 丢弃标识符
                            data = data.Substring(2);
                            // 数据定义19 3个字节、精度1
                            dataDefine1 = Convert.ToString(int.Parse(data.Substring(0, 1)), 2);
                            dataDefine2 = Convert.ToString(int.Parse(data.Substring(1, 1)), 2);
                            dataDefine = dataDefine1.PadLeft(4, '0') + dataDefine2.PadLeft(4, '0');
                            dataLength = Convert.ToInt32(dataDefine.Substring(0, 5), 2) * 2;
                            dataAccuracyLength = Convert.ToInt32(dataDefine.Substring(5, 3), 2);
                            dataAccuracy = 1;
                            while (dataAccuracyLength > 0)
                            {
                                dataAccuracy *= (decimal)0.1;
                                dataAccuracyLength--;
                            }
                            data = data.Substring(2);

                            // 根据长度精度解析数据
                            if (reportType == "31")
                            {
                                for (int i = 0; i < 13; i++)
                                {
                                    try
                                    {
                                        // 数据截取
                                        string rainString = data.Substring(0, dataLength);
                                        data = data.Substring(dataLength);
                                        if (i != 0)
                                        {
                                            dataTime = dataTime + span;
                                        }
                                        Decimal rain = 0;
                                        rain = Decimal.Parse(rainString) * dataAccuracy;

                                        // 数据封包
                                        bool isExists = false;
                                        if (datas.Count != 0)
                                        {
                                            foreach (var d in datas)
                                            {
                                                if (d.Time == dataTime)
                                                {
                                                    isExists = true;
                                                    d.Rain = rain;
                                                }
                                            }
                                        }
                                        if (isExists == false)
                                        {
                                            CReportData reportData = new CReportData
                                            {
                                                Rain = rain,
                                                Time = dataTime
                                            };
                                            datas.Add(reportData);
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        System.Diagnostics.Debug.WriteLine("规约协议数据截取错误" + e.ToString());
                                    }
                                }
                            }
                            else if (reportType == "32" || reportType == "33")
                            {
                                try
                                {
                                    // 数据截取
                                    string rainString = data.Substring(0, dataLength);
                                    data = data.Substring(dataLength);
                                    Decimal rain = 0;
                                    rain = Decimal.Parse(rainString) * dataAccuracy;

                                    // 数据封包
                                    bool isExists = false;
                                    if (datas.Count != 0)
                                    {
                                        foreach (var d in datas)
                                        {
                                            if (d.Time == dataTime)
                                            {
                                                isExists = true;
                                                d.Rain = rain;
                                            }
                                        }
                                    }
                                    if (isExists == false)
                                    {
                                        CReportData reportData = new CReportData
                                        {
                                            Rain = rain,
                                            Time = dataTime
                                        };
                                        datas.Add(reportData);
                                    }

                                }
                                catch (Exception e)
                                {
                                    System.Diagnostics.Debug.WriteLine("规约协议数据截取错误" + e.ToString());
                                }
                            }
                            break;
                        case "Z":
                            // 丢弃标识符
                            data = data.Substring(2);
                            // 丢弃数据定义23 4个字节、精度3
                            dataDefine1 = Convert.ToString(int.Parse(data.Substring(0, 1)), 2);
                            dataDefine2 = Convert.ToString(int.Parse(data.Substring(1, 1)), 2);
                            dataDefine = dataDefine1.PadLeft(4, '0') + dataDefine2.PadLeft(4, '0');
                            dataLength = Convert.ToInt32(dataDefine.Substring(0, 5), 2) * 2;
                            dataAccuracyLength = Convert.ToInt32(dataDefine.Substring(5, 3), 2);
                            dataAccuracy = 1;
                            while (dataAccuracyLength > 0)
                            {
                                dataAccuracy *= (decimal)0.1;
                                dataAccuracyLength--;
                            }
                            data = data.Substring(2);

                            // 根据长度精度解析数据
                            if (reportType == "31")
                            {
                                for (int i = 0; i < 13; i++)
                                {
                                    try
                                    {
                                        // 数据截取
                                        string waterString = data.Substring(0, dataLength);
                                        data = data.Substring(dataLength);
                                        if (i != 0)
                                        {
                                            dataTime = dataTime + span;
                                        }
                                        Decimal water = 0;
                                        water = Decimal.Parse(waterString) * dataAccuracy;

                                        // 数据封包
                                        bool isExists = false;
                                        if (datas.Count != 0)
                                        {
                                            foreach (var d in datas)
                                            {
                                                if (d.Time == dataTime)
                                                {
                                                    isExists = true;
                                                    d.Water = water;
                                                }
                                            }
                                        }
                                        if (isExists == false)
                                        {
                                            CReportData reportData = new CReportData
                                            {
                                                Water = water,
                                                Time = dataTime
                                            };
                                            datas.Add(reportData);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        System.Diagnostics.Debug.WriteLine("规约协议数据截取错误" + e.ToString());
                                    }
                                }
                            }
                            else if (reportType == "32" || reportType == "33")
                            {
                                try
                                {
                                    // 数据截取
                                    string waterString = data.Substring(0, dataLength);
                                    data = data.Substring(dataLength);
                                    Decimal water = 0;
                                    water = Decimal.Parse(waterString) * dataAccuracy;

                                    // 数据封包
                                    bool isExists = false;
                                    if (datas.Count != 0)
                                    {
                                        foreach (var d in datas)
                                        {
                                            if (d.Time == dataTime)
                                            {
                                                isExists = true;
                                                d.Water = water;
                                            }
                                        }
                                    }
                                    if (isExists == false)
                                    {
                                        CReportData reportData = new CReportData
                                        {
                                            Water = water,
                                            Time = dataTime
                                        };
                                        datas.Add(reportData);
                                    }
                                }
                                catch (Exception e)
                                {
                                    System.Diagnostics.Debug.WriteLine("规约协议数据截取错误" + e.ToString());
                                }
                            }
                            break;
                        case "VT":
                            // 丢弃标识符
                            data = data.Substring(2);
                            // 丢弃数据定义?? ?个字节、精度？
                            dataDefine1 = Convert.ToString(int.Parse(data.Substring(0, 1)), 2);
                            dataDefine2 = Convert.ToString(int.Parse(data.Substring(1, 1)), 2);
                            dataDefine = dataDefine1.PadLeft(4, '0') + dataDefine2.PadLeft(4, '0');
                            dataLength = Convert.ToInt32(dataDefine.Substring(0, 5), 2) * 2;
                            dataAccuracyLength = Convert.ToInt32(dataDefine.Substring(5, 3), 2);
                            dataAccuracy = 1;
                            while (dataAccuracyLength > 0)
                            {
                                dataAccuracy *= (decimal)0.1;
                                dataAccuracyLength--;
                            }
                            data = data.Substring(2);

                            // 根据长度精度解析数据
                            if (reportType == "31")
                            {
                                for (int i = 0; i < 13; i++)
                                {
                                    try
                                    {
                                        // 数据截取
                                        string voltageString = data.Substring(0, dataLength);
                                        data = data.Substring(dataLength);
                                        if (i != 0)
                                        {
                                            dataTime = dataTime + span;
                                        }
                                        Decimal voltage = 0;
                                        voltage = Decimal.Parse(voltageString) * dataAccuracy;

                                        // 数据封包
                                        bool isExists = false;
                                        if (datas.Count != 0)
                                        {
                                            foreach (var d in datas)
                                            {
                                                if (d.Time == dataTime)
                                                {
                                                    isExists = true;
                                                    d.Voltge = voltage;
                                                }
                                            }
                                        }
                                        if (isExists == false)
                                        {
                                            CReportData reportData = new CReportData
                                            {
                                                Voltge = voltage,
                                                Time = dataTime
                                            };
                                            datas.Add(reportData);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        System.Diagnostics.Debug.WriteLine("规约协议数据截取错误" + e.ToString());
                                    }
                                }
                            }
                            else if (reportType == "32" || reportType == "33")
                            {
                                try
                                {
                                    // 数据截取
                                    string voltageString = data.Substring(0, dataLength);
                                    data = data.Substring(dataLength);
                                    Decimal voltage = 0;
                                    voltage = Decimal.Parse(voltageString) * dataAccuracy;

                                    // 数据封包
                                    bool isExists = false;
                                    if (datas.Count != 0)
                                    {
                                        foreach (var d in datas)
                                        {
                                            if (d.Time == dataTime)
                                            {
                                                isExists = true;
                                                d.Voltge = voltage;
                                            }
                                        }
                                    }
                                    if (isExists == false)
                                    {
                                        CReportData reportData = new CReportData
                                        {
                                            Voltge = voltage,
                                            Time = dataTime
                                        };
                                        datas.Add(reportData);
                                    }
                                }
                                catch (Exception e)
                                {
                                    System.Diagnostics.Debug.WriteLine("规约协议数据截取错误" + e.ToString());
                                }
                            }
                            break;
                        default: break;
                    }
                }

                foreach (var d in datas)
                {
                    if (!d.Rain.HasValue)
                    {
                        d.Rain = -1;
                    }
                    if (!d.Water.HasValue)
                    {
                        d.Water = -20000;
                    }if(d.Voltge <= 0)
                    {
                        d.Voltge = -20;
                    }
                }

                report = new CReportStruct
                {
                    StationType = type,
                    Datas = datas
                };

                return true;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("数据：" + msg);
                System.Diagnostics.Debug.WriteLine("规约协议正文解析错误" + e.Message);
                return false;
            }
        }

        public bool Parse_1(string msg, out CReportStruct report)
        {
            throw new NotImplementedException();
        }

        public bool Parse_2(string msg, out CReportStruct report)
        {
            throw new NotImplementedException();
        }

    }
}


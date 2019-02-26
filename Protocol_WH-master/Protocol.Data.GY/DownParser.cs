using Hydrology.Entity;
using Protocol.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Data.GY
{
    class DownParser : IDown
    {
        public String BuildQuery(string sid, IList<EDownParam> cmds, EChannelType ctype)
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append(Encoding.ASCII.GetBytes("01"));//  添加首字符
            sb.Append("*");//  测试
            sb.Append(String.Format("{0:D10}", Int32.Parse(sid.Trim())));//  添加遥测站地址 
            sb.Append(String.Format("{0:D2}", Int32.Parse(sid.Trim())));//  添加中心站地址
            sb.Append(String.Format("{0:D4}", Int32.Parse(sid.Trim())));//  添加密码
            sb.Append(String.Format("{0:D2}", Int32.Parse(sid.Trim())));//  添加功能码
            //sb.Append(Encoding.ASCII.GetBytes("02"));//  添加包起始符
            sb.Append("'");//  测试
            sb.Append("0000");//  添加下行流水号
            int length = 4;//  指令的长度
            foreach (var cmd in cmds)
            {
                switch (cmd)
                {
                    case EDownParam.ontime://  发报时间
                        sb.Append(ProtocolMaps.DownParamMap.FindValue(cmd)); length += 12; break;
                    case EDownParam.oldPwd://  旧密码
                        sb.Append("03");
                        sb.Append(CSpecialChars.BALNK_CHAR);
                        sb.Append("1234");
                        sb.Append(CSpecialChars.BALNK_CHAR); length += 8; break;
                    case EDownParam.newPwd://  新密码
                        sb.Append("03");
                        sb.Append(CSpecialChars.BALNK_CHAR);
                        sb.Append("4321"); length += 7; break;
                    case EDownParam.memoryReset://  初始化固态存储
                        sb.Append("97"); length += 2; break;
                    case EDownParam.timeFrom_To://  时段起止时间
                        sb.Append(ProtocolMaps.DownParamMap.FindValue(cmd));
                        sb.Append(CSpecialChars.BALNK_CHAR); length += 17; break;

                    /*case EDownParam.timeTo://  时段结束时间  
                        sb.Append(ProtocolMaps.DownParamMap.FindValue(cmd));
                        sb.Append(CSpecialChars.BALNK_CHAR); length += 9; break;*/

                    case EDownParam.DRZ://  1 小时内 5 分钟间隔相对水位
                        sb.Append("DRZ");
                        sb.Append(ProtocolMaps.DownParamMap.FindValue(cmd));
                        sb.Append(CSpecialChars.BALNK_CHAR); length += 4; break;
                    case EDownParam.DRP://  1 小时内每 5 分钟时段雨量
                        sb.Append("DRP");
                        sb.Append(CSpecialChars.BALNK_CHAR); length += 4; break;
                    case EDownParam.Step://  时间步长码                   
                        sb.Append("DR");
                        sb.Append(ProtocolMaps.DownParamMap.FindValue(cmd));
                        sb.Append(CSpecialChars.BALNK_CHAR); length += 6; break;
                    case EDownParam.basicConfig://  遥测站基本配置读取/修改
                        /*for (var  in ) {
                            sb.Append(cmd);
                            sb.Append(CSpecialChars.BALNK_CHAR);
                            if (ProtocolMaps.DownParamMap.FindValue(cmd) != null)
                            {
                                sb.Append(CSpecialChars.BALNK_CHAR);
                                length += 4 + Int32.Parse(ProtocolMaps.DownParamLengthMap[cmd]);
                            }
                            else
                                length += 3;
                        }*/
                        break;
                    case EDownParam.operatingPara://  运行参数读取/修改
                                                  /*for (var  in ){
                                                     sb.Append(cmd);
                                                     sb.Append(CSpecialChars.BALNK_CHAR);
                                                     if (ProtocolMaps.DownParamMap.FindValue(cmd) != null)
                                                     {
                                                         sb.Append(CSpecialChars.BALNK_CHAR);
                                                         length += 4 + Int32.Parse(ProtocolMaps.DownParamLengthMap[cmd]);
                                                     }
                                                     else
                                                         length += 3;
                                                 }*/
                        break;
                    case EDownParam.Reset://  恢复出厂设置
                        sb.Append("98"); length += 2; break;
                    case EDownParam.ICconfig://  设罝遥测站IC卡状态
                        sb.Append("ZT");
                        sb.Append(CSpecialChars.BALNK_CHAR);
                        sb.Append(ProtocolMaps.DownParamMap.FindValue(cmd)); length += 11; break;
                    case EDownParam.pumpCtrl://  控制水泵状态
                        sb.Append(ProtocolMaps.DownParamMap.FindValue(cmd));
                        length += Int32.Parse(ProtocolMaps.DownParamLengthMap[cmd]); break;
                    case EDownParam.valveCtrl://  控制阀门状态
                        sb.Append(ProtocolMaps.DownParamMap.FindValue(cmd));
                        length += Int32.Parse(ProtocolMaps.DownParamLengthMap[cmd]); break;
                    case EDownParam.gateCtrl://  控制闸门状态
                        sb.Append(ProtocolMaps.DownParamMap.FindValue(cmd));
                        length += Int32.Parse(ProtocolMaps.DownParamLengthMap[cmd]); break;
                    case EDownParam.waterYield://  水量定值控制
                        sb.Append(ProtocolMaps.DownParamMap.FindValue(cmd));
                        length += 2; break;
                    default:
                        throw new Exception("设置下行指令参数错误");
                }
            }
            sb.Insert(19, String.Format("{0:D1}", 8));//  添加报文标识
            //length = 10;
            sb.Insert(20, String.Format("{0:X3}", length));//  添加报文长度

            return sb.ToString();
        }

        public String BuildQuery_Batch(string sid, ETrans trans, DateTime beginTime, EChannelType ctype)
        {
            return "";
        }

        public string BuildQuery_Flash(string sid, EStationType stationType, ETrans trans, DateTime beginTime, DateTime endTime, EChannelType ctype)
        {
            throw new NotImplementedException();
        }

        public string BuildQuery_SD(string sid, DateTime beginTime, EChannelType ctype)
        {
            throw new NotImplementedException();
        }

        public string BuildSet(string sid, IList<EDownParam> cmds, CDownConf down, EChannelType ctype)
        {
            throw new NotImplementedException();
        }

        public bool Parse(string resp, out CDownConf downConf)
        {
            throw new NotImplementedException();
        }

        public bool Parse_Batch(string msg, out CBatchStruct batch)
        {
            throw new NotImplementedException();
        }

        public bool Parse_Flash(string msg, EChannelType ctype, out CBatchStruct batch)
        {
            throw new NotImplementedException();
        }

        public bool Parse_SD(string msg, string id, out CSDStruct sd)
        {
            throw new NotImplementedException();
        }
    }
}

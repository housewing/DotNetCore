using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace prototype.Models
{
    public interface IIBFC
    {
        void Init(LoginMsg loginMsg);
        void SendMail(string content);

        List<string> GetIndexs();

        List<string> GetIndex1s();
        bool UpdateIndex1(string i1l);
        double SumIndex1();

        List<string> GetIndex2s();
        bool UpdateIndex2(string i2l);
        double SumIndex2();

        List<StockInfo> GetStockInfos();
        StockInfo GetStockInfoById(string id);
        Task<bool> AddStockInfo(StockInfo s, string StockType);
        Task<bool> UpdateStockInfo(StockInfo s);
        bool DeleteStockInfo(string id);
        double SumStockInfo();

        List<MortgageInfo> GetMortgageInfos();
        MortgageInfo GetMortgageInfoById(string id);
        Task<bool> AddMortgageInfo(MortgageInfo m);
        Task<bool> UpdateMortgageInfo(MortgageInfo m);
        bool DeleteMortgageInfo(string id);
        double SumMortgageInfo();
    }
}

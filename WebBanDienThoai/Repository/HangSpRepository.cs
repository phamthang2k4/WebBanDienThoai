
using WebBanDienThoai.Models;
using WebBanDienThoai.Repository;

namespace WebBanDienThoai.Models

{
    public class HangSpRepository : IHangSpRepository
    {
        private readonly BtlLtwQlbdtContext _context;
        public HangSpRepository(BtlLtwQlbdtContext context) 
        { 
            _context = context;
        }
        public Hang Add(Hang hangSp)
        {
            _context.Hangs.Add(hangSp);
            _context.SaveChanges();
            return hangSp;
        }

        public Hang Delete(string maHangSp)
        {
            throw new NotImplementedException();
        }

        public Hang Get(string maHangSp)
        {
            return _context.Hangs.Find(maHangSp);
        }

        public IEnumerable<Hang> GetAllLoaiSp()
        {
            return _context.Hangs;
        }

        public Hang Update(Hang hangSp)
        {
            _context.Update(hangSp);
            _context.SaveChanges();
            return hangSp;
        }
    }
}

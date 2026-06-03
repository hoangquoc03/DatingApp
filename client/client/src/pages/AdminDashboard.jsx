import { useEffect, useState } from "react";
import { Users, AlertTriangle, ShieldCheck, ShieldAlert, BarChart3, Search, Settings, LogOut, ArrowLeft } from "lucide-react";
import api from "../services/api";
import { useAuth } from "../contexts/AuthContext";
import { Link, useNavigate } from "react-router-dom";

export default function AdminDashboard() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState("overview");
  const [stats, setStats] = useState(null);
  const [users, setUsers] = useState([]);
  const [reports, setReports] = useState([]);
  const [loading, setLoading] = useState(true);

  // If user is not admin, redirect them
  useEffect(() => {
    if (user?.role !== 1) { // 1 = Admin in C# Enums.Role
      // Just to be safe, if we haven't mapped role to frontend yet, we rely on the API 403.
    }
    fetchData();
  }, [activeTab]);

  const fetchData = async () => {
    setLoading(true);
    try {
      if (activeTab === "overview") {
        const { data } = await api.get("/Admin/stats");
        setStats(data);
      } else if (activeTab === "users") {
        const { data } = await api.get("/Admin/users");
        setUsers(data);
      } else if (activeTab === "reports") {
        const { data } = await api.get("/Admin/reports");
        setReports(data);
      }
    } catch (err) {
      if (err.response?.status === 403) {
        alert("Bạn không có quyền truy cập trang quản trị.");
        navigate("/dashboard");
      }
    } finally {
      setLoading(false);
    }
  };

  const toggleUserStatus = async (id) => {
    try {
      await api.post(`/Admin/users/${id}/toggle-active`);
      fetchData();
    } catch (err) {
      alert("Lỗi: " + (err.response?.data || "Không thể thực hiện"));
    }
  };

  const resolveReport = async (id) => {
    try {
      await api.post(`/Admin/reports/${id}/resolve`);
      fetchData();
    } catch (err) {
      alert("Lỗi khi xử lý report");
    }
  };

  return (
    <div className="flex h-screen bg-[#FDFBF7] font-sans">
      {/* Sidebar */}
      <aside className="w-64 bg-white border-r border-gray-100 flex flex-col">
        <div className="p-6">
          <Link to="/dashboard" className="text-2xl font-black tracking-tighter text-[#1F2937]">
            Aura<span className="text-[#FF5C9A]">.</span><span className="text-xs ml-1 text-gray-400 font-medium">ADMIN</span>
          </Link>
        </div>
        <nav className="flex-1 px-4 space-y-2">
          <button
            onClick={() => setActiveTab("overview")}
            className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium transition-all ${
              activeTab === "overview" ? "bg-[#FF5C9A]/10 text-[#FF5C9A]" : "text-gray-500 hover:bg-gray-50 hover:text-gray-900"
            }`}
          >
            <BarChart3 className="w-5 h-5" /> Tổng quan
          </button>
          <button
            onClick={() => setActiveTab("users")}
            className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium transition-all ${
              activeTab === "users" ? "bg-[#FF5C9A]/10 text-[#FF5C9A]" : "text-gray-500 hover:bg-gray-50 hover:text-gray-900"
            }`}
          >
            <Users className="w-5 h-5" /> Người dùng
          </button>
          <button
            onClick={() => setActiveTab("reports")}
            className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium transition-all ${
              activeTab === "reports" ? "bg-[#FF5C9A]/10 text-[#FF5C9A]" : "text-gray-500 hover:bg-gray-50 hover:text-gray-900"
            }`}
          >
            <AlertTriangle className="w-5 h-5" /> Báo cáo
          </button>
        </nav>
        
        {/* User Profile & Actions */}
        <div className="p-4 border-t border-gray-100 space-y-2">
          <button
            onClick={() => navigate("/dashboard")}
            className="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium text-gray-500 hover:bg-gray-50 hover:text-gray-900 transition-all"
          >
            <ArrowLeft className="w-5 h-5" /> Trở về Aura
          </button>
          <button
            onClick={logout}
            className="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-medium text-red-500 hover:bg-red-50 transition-all"
          >
            <LogOut className="w-5 h-5" /> Đăng xuất
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 overflow-auto p-8">
        <header className="mb-8 flex justify-between items-center">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">
              {activeTab === "overview" ? "Tổng quan hệ thống" : activeTab === "users" ? "Quản lý Người dùng" : "Quản lý Báo cáo"}
            </h1>
          </div>
          <div className="flex items-center gap-4">
            <div className="flex items-center bg-white px-4 py-2 rounded-xl border border-gray-200">
              <Search className="w-4 h-4 text-gray-400 mr-2" />
              <input type="text" placeholder="Tìm kiếm..." className="bg-transparent text-sm focus:outline-none" />
            </div>
            <div className="w-10 h-10 rounded-full bg-gray-200 overflow-hidden">
              <img src={user?.avatarUrl || "https://api.dicebear.com/7.x/notionists/svg"} className="w-full h-full object-cover" />
            </div>
          </div>
        </header>

        {loading ? (
          <div className="animate-pulse space-y-4">
            <div className="h-32 bg-gray-100 rounded-3xl w-full"></div>
            <div className="h-64 bg-gray-100 rounded-3xl w-full"></div>
          </div>
        ) : (
          <div className="animate-in fade-in slide-in-from-bottom-4 duration-500">
            {activeTab === "overview" && stats && (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <div className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
                  <div className="w-12 h-12 rounded-2xl bg-blue-50 flex items-center justify-center mb-4">
                    <Users className="w-6 h-6 text-blue-500" />
                  </div>
                  <h3 className="text-sm font-medium text-gray-500">Tổng Người Dùng</h3>
                  <p className="text-3xl font-bold text-gray-900 mt-1">{stats.totalUsers}</p>
                </div>
                <div className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
                  <div className="w-12 h-12 rounded-2xl bg-green-50 flex items-center justify-center mb-4">
                    <ShieldCheck className="w-6 h-6 text-green-500" />
                  </div>
                  <h3 className="text-sm font-medium text-gray-500">User Đang Hoạt Động</h3>
                  <p className="text-3xl font-bold text-gray-900 mt-1">{stats.activeUsers}</p>
                </div>
                <div className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
                  <div className="w-12 h-12 rounded-2xl bg-purple-50 flex items-center justify-center mb-4">
                    <Heart className="w-6 h-6 text-purple-500" />
                  </div>
                  <h3 className="text-sm font-medium text-gray-500">Tổng Lượt Ghép Đôi</h3>
                  <p className="text-3xl font-bold text-gray-900 mt-1">{stats.totalMatches}</p>
                </div>
                <div className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
                  <div className="w-12 h-12 rounded-2xl bg-red-50 flex items-center justify-center mb-4">
                    <AlertTriangle className="w-6 h-6 text-red-500" />
                  </div>
                  <h3 className="text-sm font-medium text-gray-500">Báo Cáo Chưa Xử Lý</h3>
                  <p className="text-3xl font-bold text-gray-900 mt-1">{stats.unresolvedReports}</p>
                </div>
              </div>
            )}

            {activeTab === "users" && (
              <div className="bg-white rounded-3xl border border-gray-100 shadow-sm overflow-hidden">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="bg-gray-50 text-gray-500 text-xs uppercase tracking-wider">
                      <th className="p-4 font-medium">Người dùng</th>
                      <th className="p-4 font-medium">Email</th>
                      <th className="p-4 font-medium">Ngày tham gia</th>
                      <th className="p-4 font-medium">Trạng thái</th>
                      <th className="p-4 font-medium text-right">Hành động</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100 text-sm">
                    {users.map((u) => (
                      <tr key={u.id} className="hover:bg-gray-50/50 transition-colors">
                        <td className="p-4 flex items-center gap-3">
                          <img src={u.avatarUrl || "https://api.dicebear.com/7.x/notionists/svg"} className="w-10 h-10 rounded-full object-cover" />
                          <span className="font-semibold text-gray-900">{u.fullName}</span>
                          {u.role === 1 && <span className="bg-purple-100 text-purple-700 text-[10px] px-2 py-0.5 rounded-full font-bold">ADMIN</span>}
                        </td>
                        <td className="p-4 text-gray-500">{u.email}</td>
                        <td className="p-4 text-gray-500">{new Date(u.createdAt).toLocaleDateString()}</td>
                        <td className="p-4">
                          <span className={`px-3 py-1 rounded-full text-xs font-medium ${u.isActive ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"}`}>
                            {u.isActive ? "Active" : "Banned"}
                          </span>
                        </td>
                        <td className="p-4 text-right">
                          <button
                            onClick={() => toggleUserStatus(u.id)}
                            disabled={u.role === 1}
                            className={`px-4 py-2 rounded-xl text-xs font-semibold transition-all ${
                              u.isActive ? "bg-red-50 text-red-600 hover:bg-red-100" : "bg-green-50 text-green-600 hover:bg-green-100"
                            } disabled:opacity-30 disabled:cursor-not-allowed`}
                          >
                            {u.isActive ? "Khóa" : "Mở khóa"}
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            {activeTab === "reports" && (
              <div className="space-y-4">
                {reports.length === 0 ? (
                  <div className="text-center py-16 bg-white rounded-3xl border border-gray-100">
                    <ShieldCheck className="w-12 h-12 text-green-400 mx-auto mb-4" />
                    <p className="text-gray-500 font-medium">Hệ thống đang rất an toàn, không có báo cáo nào.</p>
                  </div>
                ) : reports.map(r => (
                  <div key={r.id} className="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm flex flex-col md:flex-row md:items-start justify-between gap-6 hover:shadow-md transition-shadow">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-4">
                        <span className={`px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider ${
                          r.status === "pending" ? "bg-red-100 text-red-700" : "bg-green-100 text-green-700"
                        }`}>
                          {r.status}
                        </span>
                        <span className="text-gray-400 text-xs font-medium">{new Date(r.createdAt).toLocaleString()}</span>
                      </div>
                      
                      <div className="flex items-center gap-4 mb-4 bg-gray-50 p-4 rounded-2xl">
                        <div className="flex items-center gap-2">
                          <img src={r.reporter.avatarUrl || "https://api.dicebear.com/7.x/notionists/svg"} className="w-8 h-8 rounded-full border-2 border-white shadow-sm object-cover" />
                          <span className="text-sm font-medium text-gray-700">{r.reporter.fullName}</span>
                        </div>
                        <ArrowLeft className="w-4 h-4 text-gray-400 rotate-180" />
                        <div className="flex items-center gap-2">
                          <img src={r.reportedUser.avatarUrl || "https://api.dicebear.com/7.x/notionists/svg"} className="w-8 h-8 rounded-full border-2 border-red-100 shadow-sm object-cover" />
                          <span className="text-sm font-bold text-red-600">{r.reportedUser.fullName}</span>
                        </div>
                      </div>

                      <div className="space-y-2">
                        <div className="inline-flex items-center gap-2 text-sm font-semibold text-gray-900">
                          <AlertTriangle className="w-4 h-4 text-orange-500" />
                          Vi phạm: {r.reason}
                        </div>
                        {r.description && <p className="text-gray-600 text-sm italic pl-6 border-l-2 border-gray-200">"{r.description}"</p>}
                      </div>
                    </div>
                    
                    <div className="flex flex-col gap-3 min-w-[140px]">
                      {r.status === "pending" && (
                        <button 
                          onClick={() => resolveReport(r.id)}
                          className="w-full px-6 py-3 bg-gray-900 text-white rounded-xl text-sm font-medium hover:bg-gray-800 transition-colors shadow-sm"
                        >
                          Đánh dấu Xử lý
                        </button>
                      )}
                      <button 
                          onClick={() => {
                             setActiveTab("users"); 
                             // Could implement filtering here, but for now just take them to users tab
                          }}
                          className="w-full px-6 py-3 bg-red-50 text-red-600 rounded-xl text-sm font-medium hover:bg-red-100 transition-colors"
                        >
                          Xử phạt User
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </main>
    </div>
  );
}
// Heart icon mock since it wasn't imported
function Heart(props) {
  return <svg {...props} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M19 14c1.49-1.46 3-3.21 3-5.5A5.5 5.5 0 0 0 16.5 3c-1.76 0-3 .5-4.5 2-1.5-1.5-2.74-2-4.5-2A5.5 5.5 0 0 0 2 8.5c0 2.3 1.5 4.05 3 5.5l7 7Z"/></svg>;
}

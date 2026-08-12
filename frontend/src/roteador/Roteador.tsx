import { Route, Routes } from 'react-router-dom';
import LayoutPrincipal from '../compartilhado/layouts/LayoutPrincipal';
import PaginaResumo from '../modulos/resumo/paginas/PaginaResumo';
import PaginaProdutos from '../modulos/produtos/paginas/PaginaProdutos';
import PaginaClientes from '../modulos/clientes/paginas/PaginaClientes';
import PaginaCompras from '../modulos/compras/paginas/PaginaCompras';
import PaginaVendas from '../modulos/vendas/paginas/PaginaVendas';

/** Rotas da aplicação. Como o menu, é ponto de conflito quando duas features chegam juntas. */
export default function Roteador() {
  return (
    <Routes>
      <Route path="/" element={<LayoutPrincipal />}>
        <Route index element={<PaginaResumo />} />
        <Route path="produtos" element={<PaginaProdutos />} />
        <Route path="clientes" element={<PaginaClientes />} />
        <Route path="compras" element={<PaginaCompras />} />
        <Route path="vendas" element={<PaginaVendas />} />
      </Route>
    </Routes>
  );
}

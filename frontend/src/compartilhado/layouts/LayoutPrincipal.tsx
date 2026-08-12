import { Layout, Menu, Typography } from 'antd';
import type { MenuProps } from 'antd';
import { Link, Outlet, useLocation } from 'react-router-dom';

const { Header, Content, Sider } = Layout;

/**
 * Menu lateral. Duas features que adicionam item aqui ao mesmo tempo conflitam neste array —
 * é o conflito de merge mais provável do projeto, e ele é fácil de resolver de propósito.
 */
const itens: MenuProps['items'] = [
  { key: '/', label: <Link to="/">Resumo</Link> },
  { key: '/produtos', label: <Link to="/produtos">Produtos</Link> },
  { key: '/compras', label: <Link to="/compras">Compras</Link> },
  { key: '/vendas', label: <Link to="/vendas">Vendas</Link> },
];

export default function LayoutPrincipal() {
  const local = useLocation();

  // "/compras/abc" deve manter "Compras" marcado. A raiz é caso à parte, senão casaria com tudo.
  const selecionado =
    itens
      ?.map((item) => String(item?.key))
      .filter((chave) => chave !== '/' && local.pathname.startsWith(chave)) ?? [];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center' }}>
        <Typography.Title level={4} style={{ color: '#fff', margin: 0 }}>
          Projeto Vendas Teste
        </Typography.Title>
      </Header>
      <Layout>
        <Sider width={200} breakpoint="lg" collapsedWidth={0}>
          <Menu
            mode="inline"
            selectedKeys={selecionado.length > 0 ? [selecionado[0]] : ['/']}
            items={itens}
            style={{ height: '100%' }}
          />
        </Sider>
        <Content style={{ padding: 24 }}>
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  );
}

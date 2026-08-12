import { ConfigProvider, App as AppAntd } from 'antd';
import ptBR from 'antd/locale/pt_BR';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import dayjs from 'dayjs';
import 'dayjs/locale/pt-br';
import Roteador from './roteador/Roteador';

dayjs.locale('pt-br');

const clienteQuery = new QueryClient({
  defaultOptions: {
    queries: {
      // Sem refetch ao voltar para a aba: é um sistema de lançamento, não um painel ao vivo.
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

export default function App() {
  return (
    <ConfigProvider locale={ptBR}>
      <AppAntd>
        <QueryClientProvider client={clienteQuery}>
          <BrowserRouter>
            <Roteador />
          </BrowserRouter>
        </QueryClientProvider>
      </AppAntd>
    </ConfigProvider>
  );
}

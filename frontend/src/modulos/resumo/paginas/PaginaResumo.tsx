import { useState } from 'react';
import { Card, Col, DatePicker, Row, Space, Statistic, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useQuery } from '@tanstack/react-query';
import type { Dayjs } from 'dayjs';
import { resumoApi } from '../../movimentos/servicos/movimentosApi';
import type { CompraResumo, VendaResumo } from '../../movimentos/servicos/movimentosApi';
import { formatarData, formatarMoeda } from '../../../compartilhado/utils/formatadores';

export default function PaginaResumo() {
  const [periodo, setPeriodo] = useState<[Dayjs, Dayjs] | null>(null);

  const filtro = periodo
    ? { dataInicial: periodo[0].format('YYYY-MM-DD'), dataFinal: periodo[1].format('YYYY-MM-DD') }
    : {};

  const { data: resumo, isFetching } = useQuery({
    queryKey: ['resumo', filtro],
    queryFn: () => resumoApi.obter(filtro),
  });

  const colunasCompras: ColumnsType<CompraResumo> = [
    { title: 'Nº', dataIndex: 'numero', width: 80 },
    { title: 'Data', dataIndex: 'data', width: 120, render: (iso: string) => formatarData(iso) },
    { title: 'Fornecedor', dataIndex: 'fornecedor' },
    {
      title: 'Valor',
      dataIndex: 'valorTotal',
      width: 140,
      align: 'right',
      render: (valor: number) => formatarMoeda(valor),
    },
  ];

  const colunasVendas: ColumnsType<VendaResumo> = [
    { title: 'Nº', dataIndex: 'numero', width: 80 },
    { title: 'Data', dataIndex: 'data', width: 120, render: (iso: string) => formatarData(iso) },
    { title: 'Cliente', dataIndex: 'cliente' },
    {
      title: 'Valor',
      dataIndex: 'valorTotal',
      width: 140,
      align: 'right',
      render: (valor: number) => formatarMoeda(valor),
    },
  ];

  const diferenca = resumo?.diferenca ?? 0;

  return (
    <Space direction="vertical" size="middle" style={{ width: '100%' }}>
      <Card title="Resumo" loading={isFetching}>
        <DatePicker.RangePicker
          format="DD/MM/YYYY"
          style={{ marginBottom: 16 }}
          onChange={(valores) =>
            setPeriodo(valores && valores[0] && valores[1] ? [valores[0], valores[1]] : null)
          }
        />

        <Row gutter={[16, 16]}>
          <Col xs={12} md={6}>
            <Statistic
              title={`Comprado (${resumo?.quantidadeCompras ?? 0})`}
              value={resumo?.totalComprado ?? 0}
              formatter={(valor) => formatarMoeda(Number(valor))}
            />
          </Col>
          <Col xs={12} md={6}>
            <Statistic
              title={`Vendido (${resumo?.quantidadeVendas ?? 0})`}
              value={resumo?.totalVendido ?? 0}
              formatter={(valor) => formatarMoeda(Number(valor))}
            />
          </Col>
          <Col xs={12} md={6}>
            <Statistic
              title="Vendas − compras"
              value={diferenca}
              valueStyle={{ color: diferenca >= 0 ? '#3f8600' : '#cf1322' }}
              formatter={(valor) => formatarMoeda(Number(valor))}
            />
          </Col>
          <Col xs={12} md={6}>
            <Statistic
              title="Produtos (ativos)"
              value={`${resumo?.produtosCadastrados ?? 0} (${resumo?.produtosAtivos ?? 0})`}
            />
          </Col>
        </Row>
      </Card>

      <Row gutter={16}>
        <Col xs={24} lg={12}>
          <Card title="Últimas compras" size="small">
            <Table
              rowKey="id"
              size="small"
              columns={colunasCompras}
              dataSource={resumo?.ultimasCompras ?? []}
              pagination={false}
              locale={{ emptyText: 'Nenhuma compra no período.' }}
            />
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Últimas vendas" size="small">
            <Table
              rowKey="id"
              size="small"
              columns={colunasVendas}
              dataSource={resumo?.ultimasVendas ?? []}
              pagination={false}
              locale={{ emptyText: 'Nenhuma venda no período.' }}
            />
          </Card>
        </Col>
      </Row>
    </Space>
  );
}

import { useEffect } from 'react';
import { Alert, Form, Input, InputNumber, Modal, Switch } from 'antd';
import type { Produto, SalvarProdutoRequest } from '../servicos/produtosApi';

interface Props {
  aberto: boolean;
  produto: Produto | null;
  salvando: boolean;
  aoCancelar: () => void;
  aoSalvar: (dados: SalvarProdutoRequest) => void;
}

export default function ModalProduto({ aberto, produto, salvando, aoCancelar, aoSalvar }: Props) {
  const [form] = Form.useForm<SalvarProdutoRequest>();

  useEffect(() => {
    if (!aberto) {
      return;
    }

    if (produto) {
      form.setFieldsValue(produto);
    } else {
      form.resetFields();
      form.setFieldsValue({ unidadeMedida: 'UN', precoCusto: 0, precoVenda: 0, ativo: true });
    }
  }, [aberto, produto, form]);

  // Preço de venda abaixo do custo é permitido (decisão de escopo) — a tela apenas avisa.
  const custo = Form.useWatch('precoCusto', form) ?? 0;
  const venda = Form.useWatch('precoVenda', form) ?? 0;
  const abaixoDoCusto = venda > 0 && venda < custo;

  return (
    <Modal
      open={aberto}
      title={produto ? 'Editar produto' : 'Novo produto'}
      okText="Salvar"
      cancelText="Cancelar"
      confirmLoading={salvando}
      onCancel={aoCancelar}
      onOk={() => form.submit()}
      destroyOnHidden
    >
      <Form form={form} layout="vertical" onFinish={aoSalvar} disabled={salvando}>
        <Form.Item
          name="codigo"
          label="Código"
          rules={[{ required: true, message: 'Informe o código.' }, { max: 20 }]}
        >
          <Input placeholder="Ex.: BAN001" />
        </Form.Item>

        <Form.Item
          name="nome"
          label="Nome"
          rules={[{ required: true, message: 'Informe o nome.' }, { max: 120 }]}
        >
          <Input placeholder="Ex.: Banana Prata" />
        </Form.Item>

        <Form.Item
          name="unidadeMedida"
          label="Unidade de medida"
          rules={[{ required: true, message: 'Informe a unidade.' }, { max: 10 }]}
        >
          <Input placeholder="UN, KG, CX…" />
        </Form.Item>

        <Form.Item
          name="precoCusto"
          label="Preço de custo"
          rules={[{ required: true, message: 'Informe o preço de custo.' }]}
        >
          <InputNumber min={0} precision={2} decimalSeparator="," style={{ width: '100%' }} />
        </Form.Item>

        <Form.Item
          name="precoVenda"
          label="Preço de venda"
          rules={[{ required: true, message: 'Informe o preço de venda.' }]}
        >
          <InputNumber min={0} precision={2} decimalSeparator="," style={{ width: '100%' }} />
        </Form.Item>

        {abaixoDoCusto && (
          <Alert
            type="warning"
            showIcon
            style={{ marginBottom: 16 }}
            message="O preço de venda está abaixo do preço de custo."
          />
        )}

        <Form.Item name="ativo" label="Ativo" valuePropName="checked">
          <Switch />
        </Form.Item>
      </Form>
    </Modal>
  );
}

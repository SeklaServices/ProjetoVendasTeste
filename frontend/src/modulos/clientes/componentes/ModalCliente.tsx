import { useEffect } from 'react';
import { Form, Input, Modal, Switch } from 'antd';
import type { Cliente, SalvarClienteRequest } from '../servicos/clientesApi';

interface Props {
  aberto: boolean;
  cliente: Cliente | null;
  salvando: boolean;
  aoCancelar: () => void;
  aoSalvar: (dados: SalvarClienteRequest) => void;
}

export default function ModalCliente({ aberto, cliente, salvando, aoCancelar, aoSalvar }: Props) {
  const [form] = Form.useForm<SalvarClienteRequest>();

  useEffect(() => {
    if (!aberto) {
      return;
    }

    if (cliente) {
      form.setFieldsValue(cliente);
    } else {
      form.resetFields();
      form.setFieldsValue({ ativo: true });
    }
  }, [aberto, cliente, form]);

  return (
    <Modal
      open={aberto}
      // O código não aparece no formulário: é gerado pelo banco e não se altera.
      title={cliente ? `Editar cliente nº ${cliente.codigo}` : 'Novo cliente'}
      okText="Salvar"
      cancelText="Cancelar"
      confirmLoading={salvando}
      onCancel={aoCancelar}
      onOk={() => form.submit()}
      destroyOnHidden
    >
      <Form form={form} layout="vertical" onFinish={aoSalvar} disabled={salvando}>
        <Form.Item
          name="nome"
          label="Nome"
          rules={[{ required: true, message: 'Informe o nome.' }, { max: 120 }]}
        >
          <Input placeholder="Ex.: Mercado Central" />
        </Form.Item>

        <Form.Item
          name="documento"
          label="CPF ou CNPJ"
          extra="Opcional. Aceita com ou sem pontuação — o sistema guarda só os dígitos."
          rules={[
            {
              validator: (_, valor: string | null | undefined) => {
                if (!valor || valor.trim() === '') {
                  return Promise.resolve();
                }

                const digitos = valor.replace(/\D/g, '');
                return digitos.length === 11 || digitos.length === 14
                  ? Promise.resolve()
                  : Promise.reject(new Error('Informe um CPF (11 dígitos) ou CNPJ (14 dígitos).'));
              },
            },
          ]}
        >
          <Input placeholder="000.000.000-00" />
        </Form.Item>

        <Form.Item name="telefone" label="Telefone" rules={[{ max: 20 }]}>
          <Input placeholder="(00) 00000-0000" />
        </Form.Item>

        <Form.Item
          name="email"
          label="E-mail"
          rules={[{ type: 'email', message: 'O e-mail informado não é válido.' }, { max: 120 }]}
        >
          <Input placeholder="contato@empresa.com.br" />
        </Form.Item>

        <Form.Item
          name="ativo"
          label="Ativo"
          valuePropName="checked"
          extra="Cliente inativo não aparece na seleção de vendas."
        >
          <Switch />
        </Form.Item>
      </Form>
    </Modal>
  );
}
